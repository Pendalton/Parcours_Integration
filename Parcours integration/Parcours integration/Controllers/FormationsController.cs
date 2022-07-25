using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Parcours_integration.Models;

namespace Parcours_integration.Controllers
{
    public class FormationsController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();
        // GET: Formations
        public ActionResult Index(int? Numéro, bool CDI = true, bool CDD = true, bool Stage = true, bool Mutation = true, bool Intérim = true)
        {
            if (!EstFormateur)
            {
                var Nom = UserSession.Nom;
                var Parc = db.Parcours.Where(m => m.Nom + m.Prénom == Nom).FirstOrDefault();

                if(Parc == null)
                {
                    return RedirectToAction("Index", "Parcours");
                }
                else 
                { 
                    return RedirectToAction("Details", "Parcours", new { id = Parc.ID }); 
                }
            }

            var SectEmploi = UserService.Select(s=>s.Service.Nom).ToList();

            var missions = new List<IQueryable>();

            var Parcours = from m in db.Parcours.Where(s => s.Complété == false)
                             select m;

            List<Parcours> Résultat = new List<Parcours>();

            if (CDI)
            {
                Résultat.AddRange(Parcours.Where(s => s.Type_Contrat == 1));
            }
            if (CDD)
            {
                Résultat.AddRange(Parcours.Where(s => s.Type_Contrat == 2));
            }
            if (Stage)
            {
                Résultat.AddRange(Parcours.Where(s => s.Type_Contrat == 3));
            }
            if (Mutation)
            {
                Résultat.AddRange(Parcours.Where(s => s.Type_Contrat == 4));
            }
            if (Intérim)
            {
                Résultat.AddRange(Parcours.Where(s => s.Type_Contrat == 7));
            }

            if (Numéro != null)
            {
                Résultat = Résultat.Where(s => s.ID == Numéro).ToList();
            }

            if (!UserSession.EstResponsable)
            {
                foreach (var item in Résultat.ToList())
                {
                    var Miss = db.Missions.Where(s => s.ID_Parcours == item.ID).Where(s => SectEmploi.Contains(s.Nom_Secteur)).Where(s=>s.Applicable==true).Where(s => s.Passage == false);
                    if (Miss.Count() == 0)
                    {
                        Résultat.Remove(item);
                    }
                    Miss = Miss.OrderBy(s => s.Nom_Mission);
                    missions.Add(Miss);
                    item.Date_entrée = item.Entrée.ToString().Substring(0, 10);
                }
            }
            else
            {
                foreach (var item in Résultat.ToList())
                {
                    if(item.ID_Resp == UserSession.ID)
                    {
                        var RespService = db.Service.Find(7).Nom;
                        var Miss = db.Missions.Where(s => s.ID_Parcours == item.ID).Where(s => SectEmploi.Contains(s.Nom_Secteur) || s.Nom_Secteur == RespService).Where(s => s.Applicable == true).Where(s => s.Passage == false);
                        if (Miss.Count() == 0)
                        {
                            Résultat.Remove(item);
                        }
                        Miss = Miss.OrderBy(s => s.Nom_Mission);
                        missions.Add(Miss);
                        item.Date_entrée = item.Entrée.ToString().Substring(0, 10);
                    }
                    else
                    {
                        var Miss = db.Missions.Where(s => s.ID_Parcours == item.ID).Where(s => SectEmploi.Contains(s.Nom_Secteur)).Where(s => s.Applicable == true).Where(s => s.Passage == false);
                        if (Miss.Count() == 0)
                        {
                            Résultat.Remove(item);
                        }
                        Miss = Miss.OrderBy(s => s.Nom_Mission);
                        missions.Add(Miss);
                        item.Date_entrée = item.Entrée.ToString().Substring(0, 10);
                    }
                }
            }

            ViewBag.Missions = missions;

            ViewBag.CDI = CDI;
            ViewBag.CDD = CDD;
            ViewBag.Stage = Stage;
            ViewBag.Mutation = Mutation;
            ViewBag.Intérim = Intérim;

            Résultat = Résultat.OrderBy(s => s.ID).ToList();
            return View(Résultat);
        }

        [HttpPost]
        public ActionResult Sign(FormCollection form)
        {
            var Rem = form["RemarqueTxt"].ToString();
            var MissionID = int.Parse(form["MissionID"]);

            Missions missions = db.Missions.Find(MissionID);

            if (ModelState.IsValid)
            {
                missions.Passage = true;
                missions.Date_passage = DateTime.Now.Date.ToString().Substring(0, 10);
                missions.ID_Formateur = UserSession.ID;
                missions.Remarque = Rem;

                db.Entry(missions).State = EntityState.Modified;
                db.SaveChanges();

                var MissComp = db.Missions.Where(s => s.ID_Parcours == missions.ID_Parcours).Where(s => s.Passage == false).FirstOrDefault();
                if (MissComp == null)
                {
                    Parcours parc = db.Parcours.Find(missions.ID_Parcours);
                    parc.Complété = true;
                    parc.Utilisateurs.EstResponsable = false;
                }
                else
                {
                    Parcours parc = db.Parcours.Find(missions.ID_Parcours);
                    parc.Complété = false;
                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(missions);
        }

        public ActionResult Plan(int ID)
        {
            var Miss = db.Missions.Find(ID);

            Miss.Planifié = !Miss.Planifié;
            db.Entry(Miss).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}