using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Parcours_integration.Models;
using System.Dynamic;

namespace Parcours_integration.Controllers
{
    public class FormationsController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();
        // GET: Formations
        public ActionResult Index(int? Numéro, bool CDI = true, bool CDD = true, bool Stage = true, bool Mutation = true, bool Intérim = true)
        {
            dynamic ListAFaire = new ExpandoObject();

            if (!EstFormateur && !EstResponsable)
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

            var missions = new List<Missions>();

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

            foreach (var item in Résultat.ToList())
            {
                var Responsable = db.Service.Find(7).Nom;

                var Miss = db.Missions.Where(s=>!s.Passage && s.Applicable && s.ID_Parcours == item.ID);

                if (EstResponsable && item.ID_Resp == UserSession.ID)
                {
                    Miss = Miss.Where(s => SectEmploi.Contains(s.Nom_Secteur) || s.Nom_Secteur == Responsable);
                }
                else
                {
                    Miss = Miss.Where(s => SectEmploi.Contains(s.Nom_Secteur));
                }

                if (Miss.Count() == 0)
                {
                    Résultat.Remove(item);
                }
                else
                {
                    missions.AddRange(Miss.OrderBy(s => s.Nom_Mission));
                }
            }

            {
                ViewBag.Missions = missions.ToList();

                ViewBag.CDI = CDI;

                ViewBag.CDD = CDD;

                ViewBag.Stage = Stage;

                ViewBag.Mutation = Mutation;

                ViewBag.Intérim = Intérim;

                Résultat = Résultat.OrderBy(s => s.ID).ToList();
            }

            ListAFaire.Formations = Résultat;

            var SignaturesAFaire = new List<Tuple<int, string, string, bool, bool, bool, int?>>();

            foreach (var Parc in db.Parcours.Where(s => s.Complété).ToList())
            {
                if (db.Signatures.Where(s => s.ID_Parcours == Parc.ID).Count() < 3)
                {
                    if (Parc.ID_Resp!=UserSession.ID && !(EstAdmin || EstManPower))
                    {
                        continue;
                    }
                    else
                    {
                        var RH = db.Signatures.Where(s => s.ID_Parcours == Parc.ID).Any(s => s.Role == "Ressources Humaines");
                        var Resp = db.Signatures.Where(s => s.ID_Parcours == Parc.ID).Any(s => s.Role == "Responsable");
                        var Emp = db.Signatures.Where(s => s.ID_Parcours == Parc.ID).Any(s => s.Role == "Employé" || s.Role == "Admin");

                        var PASIGNER = Tuple.Create(Parc.ID, Parc.Nom + " " + Parc.Prénom, Parc.Poste + " - " + Parc.Contrat.Nom, RH, Resp, Emp, Parc.ID_Resp);
                        SignaturesAFaire.Add(PASIGNER);
                    }
                }
            }

            ListAFaire.Signatures = SignaturesAFaire;

            return View(ListAFaire);
        }

        [HttpPost]
        public bool Sign(int ID, string Rem)
        {
            Missions missions = db.Missions.Find(ID);

            if (ModelState.IsValid)
            {
                missions.Passage = true;
                missions.Date_passage = DateTime.Now.Date.ToString().Substring(0, 10);
                missions.ID_Formateur = UserSession.ID;
                missions.Remarque = Rem;

                db.Entry(missions).State = EntityState.Modified;
                db.SaveChanges();

                var MissComp = db.Missions.Where(s => s.ID_Parcours == missions.ID_Parcours).Where(s => !s.Passage).Where(s=>s.Applicable).Count();
                if (MissComp == 0)
                {
                    missions.Parcours.Complété = true;
                    if (db.Parcours.Where(s => s.ID_Resp == missions.Parcours.ID_Resp).Where(s => !s.Complété).Count() == 0)
                    {
                        missions.Parcours.Utilisateurs.EstResponsable = false;
                    }
                }
                else
                {
                    missions.Parcours.Complété = false;
                    if (db.Parcours.Where(s => s.ID_Resp == missions.Parcours.ID_Resp).Where(s => !s.Complété).Count() != 0)
                    {
                        missions.Parcours.Utilisateurs.EstResponsable = true;
                    }
                }

                db.SaveChanges();

                return true;
            }
            return false;
        }

        public bool Save(int ID, string Rem)
        {
            Missions missions = db.Missions.Find(ID);

            if (ModelState.IsValid)
            {
                missions.Remarque = Rem;

                db.Entry(missions).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool Plan(int ID)
        {
            var Miss = db.Missions.Find(ID);

            Miss.Planifié = !Miss.Planifié;
            db.Entry(Miss).State = EntityState.Modified;
            db.SaveChanges();

            return Miss.Planifié;
        }
    }
}