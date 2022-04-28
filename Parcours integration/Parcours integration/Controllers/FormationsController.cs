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
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();
        // GET: Formations
        public ActionResult Index(bool? CDI, bool? CDD, bool? Stage, bool? Mutation)
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

            var SectEmploi = UserSession.Secteur;
            var Parcours = db.Parcours.Where(s => s.Complété == false);
            var missions = new List<IQueryable>();

            if (CDI == true)
            {
                Parcours = Parcours.Where(s => s.Type_Contrat == 1);
            }
            if (CDD == true)
            {
                Parcours = Parcours.Where(s => s.Type_Contrat == 2);
            }
            if (Stage == true)
            {
                Parcours = Parcours.Where(s => s.Type_Contrat == 3);
            }
            if (Mutation == true)
            {
                Parcours = Parcours.Where(s => s.Type_Contrat == 4);
            }

            foreach(var item in Parcours)
            {
                var Miss = db.Missions.Where(s => s.ID_Parcours == item.ID).Where(s => s.Nom_Secteur == SectEmploi);
                if(Miss == null)
                {
                    Parcours = Parcours.Where(s => s.ID != item.ID);
                }
                Miss = Miss.OrderBy(s => s.Nom_Mission);
                missions.Add(Miss);
            }
            foreach (var item in Parcours)
            {
                var jour = item.Date_entrée.Substring(8, 2);
                var mois = item.Date_entrée.Substring(5, 2);
                var année = item.Date_entrée.Substring(0, 4);
                item.Date_entrée = jour + "/" + mois + "/" + année;
            }


            Parcours = Parcours.OrderBy(s => s.Date_entrée);
            ViewBag.Missions = missions;
            return View(Parcours);
        }

        public ActionResult Signer(int? id)
        {
            if (id == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            Missions missions = db.Missions.Find(id);
            if (missions == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return View(missions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signer([Bind(Include = "ID,Nom_Mission,Nom_Secteur,Login_Interlocuteur,Date_passage,Passage,ID_Parcours,Remarque")] Missions missions)
        {
            if (ModelState.IsValid)
            {
                missions.Passage = true;
                var Date = DateTime.Now.Date;
                var jour = Date.ToString().Substring(0, 2);
                var mois = Date.ToString().Substring(3, 2);
                var année = Date.ToString().Substring(6, 4);
                missions.Date_passage = jour + "/" + mois + "/" + année;
                missions.Login_Interlocuteur = UserSession.Login;

                db.Entry(missions).State = EntityState.Modified;
                db.SaveChanges();

                var MissComp = db.Missions.Where(s => s.ID_Parcours == missions.ID_Parcours).Where(s => s.Passage == false).FirstOrDefault();
                if (MissComp == null)
                {
                    Parcours parc = db.Parcours.Find(missions.ID_Parcours);
                    parc.Complété = true;
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
    }
}