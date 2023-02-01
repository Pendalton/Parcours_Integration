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
            if (EstResponsable)
            {
                SectEmploi.Add(db.Service.Find(7).Nom);
            }

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
                var Miss = db.Missions.Where(s => s.ID_Parcours == item.ID).Where(s => SectEmploi.Contains(s.Nom_Secteur)).Where(s => s.Applicable).Where(s => !s.Passage);
                if (Miss.Count() == 0)
                {
                    Résultat.Remove(item);
                }
                else
                {
                    Miss = Miss.OrderBy(s => s.Nom_Mission);
                    if (item.ID_Resp != UserSession.ID)
                    {
                        foreach (var truc in Miss.Where(s=>s.Nom_Secteur != Responsable))
                        {
                            missions.Add(truc);
                        }
                    }
                    else
                    {
                        foreach (var truc in Miss)
                        {
                            missions.Add(truc);
                        }
                    }
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
                    var RH = db.Signatures.Where(s => s.ID_Parcours == Parc.ID).Any(s => s.Role == "Ressources Humaines");
                    var Resp = db.Signatures.Where(s => s.ID_Parcours == Parc.ID).Any(s => s.Role == "Responsable");
                    var Emp = db.Signatures.Where(s => s.ID_Parcours == Parc.ID).Any(s => s.Role == "Employé" || s.Role == "Admin");

                    var PASIGNER = Tuple.Create(Parc.ID, Parc.Nom + " " + Parc.Prénom, Parc.Poste + " - " + Parc.Contrat.Nom, RH, Resp, Emp, Parc.ID_Resp);
                    SignaturesAFaire.Add(PASIGNER);
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