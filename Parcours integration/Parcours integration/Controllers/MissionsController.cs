﻿using System;
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
    public class MissionsController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        // GET: Missions/Create
        public ActionResult Create(int ID)
        {
            Parcours parc = db.Parcours.Find(ID);
            if (!EstAdmin)
            {
                return View("Index", "Parcours");
            }
            if (parc == null)
            {
                return View("Index", "Parcours");
            }

            var Mods = from m in db.ModeleContrat
                       select m.Modele;
            List<Modele> MissPossibles = new List<Modele>();

            MissPossibles.AddRange(Mods.Distinct());

            var MissPrésentes = db.Missions.Where(s => s.ID_Parcours == ID);

            foreach (var Mission in MissPrésentes)
            {
                foreach (var item in MissPossibles.ToList())
                {
                    if (Mission.Nom_Mission == item.Nom)
                    {
                        MissPossibles.Remove(item);
                    }
                }
            }
            ViewBag.ID = ID;
            ViewBag.ChoixMiss = new SelectList(MissPossibles, "ID", "Nom");
            ViewBag.Service = new SelectList(db.Service.Where(s => s.Actif == true), "ID", "Nom");
            return View(new Missions());
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom_Mission,Nom_Secteur,Login_Interlocuteur,Date_passage,Passage,ID_Parcours")] Missions missions, int ID, int ChoixMiss)
        {
            missions.ID_Parcours = ID;
            missions.Date_passage = "--/--/----";
            var choix = db.Modele.Find(ChoixMiss);

            missions.Nom_Mission = choix.Nom;
            missions.Nom_Secteur = choix.Service.Nom;

            if (ModelState.IsValid)
            {
                db.Missions.Add(missions);
                db.SaveChanges();
                return RedirectToAction("Details","Parcours", new { id = ID });
            }

            var Mods = from m in db.ModeleContrat
                       select m.Modele;
            List<Modele> MissPossibles = new List<Modele>();

            MissPossibles.AddRange(Mods.Distinct());

            var MissPrésentes = db.Missions.Where(s => s.ID_Parcours == ID);

            foreach (var Mission in MissPrésentes)
            {
                foreach (var item in MissPossibles.ToList())
                {
                    if (Mission.Nom_Mission == item.Nom)
                    {
                        MissPossibles.Remove(item);
                    }
                }
            }
            ViewBag.ID = ID;
            ViewBag.ChoixMiss = new SelectList(MissPossibles, "ID", "Nom");
            ViewBag.Secteur = new SelectList(db.Service.Where(s => s.Actif == true), "ID", "Nom");
            return View(missions);
        }

        public ActionResult _NewMission(string Nom_Mission, int Secteur, int ID) 
        {
            var Sect = db.Service.Find(Secteur);

            Missions newMiss = new Missions
            {
                ID_Parcours = ID,
                Nom_Mission = Nom_Mission,
                Nom_Secteur = Sect.Nom,
                Date_passage = "--/--/----"
            };


            if (ModelState.IsValid)
            {
                db.Missions.Add(newMiss);
                db.SaveChanges();
                return RedirectToAction("Details", "Parcours", new { id = ID });
            }
            return RedirectToAction("Create");
        }

        // GET: Missions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Missions missions = db.Missions.Find(id);
            if (missions == null)
            {
                return HttpNotFound();
            }
            ViewBag.Login_Interlocuteur = new SelectList(db.Utilisateurs, "Login", "Nom", missions.ID_Formateur);
            ViewBag.Service = new SelectList(db.Service.Where(s=>s.Actif==true), "ID", "Nom", missions.Nom_Secteur);
            return View(missions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom_Mission,Nom_Secteur,Login_Interlocuteur,Date_passage,Passage,ID_Parcours,Remarque")] Missions missions, int Secteurs)
        {
            var sect = db.Service.Find(Secteurs);
            missions.Nom_Secteur = sect.Nom;

            if (ModelState.IsValid)
            {
                db.Entry(missions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Parcours", new { id = missions.ID_Parcours });
            }
            ViewBag.Login_Interlocuteur = new SelectList(db.Utilisateurs, "Login", "Mail", missions.ID_Formateur);
            ViewBag.ID_Parcours = new SelectList(db.Parcours, "ID", "Nom", missions.ID_Parcours);
            ViewBag.Service = new SelectList(db.Service.Where(s => s.Actif == true), "ID", "Nom", missions.Nom_Secteur);
            return View(missions);
        }

        // GET: Missions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Missions missions = db.Missions.Find(id);
            if (missions == null)
            {
                return HttpNotFound();
            }
            return View(missions);
        }

        // POST: Missions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Missions missions = db.Missions.Find(id);
            db.Missions.Remove(missions);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Compléter(int id)
        {
            if (!EstAdmin)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            Missions Miss = db.Missions.Find(id);
            if(Miss == null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            else
            {
                if (!Miss.Passage)
                {
                    Miss.Passage = true;
                    Miss.Date_passage = DateTime.Now.Date.ToString().Substring(0, 10);
                    Miss.ID_Formateur = UserSession.ID;
                }
                else
                {
                    Miss.Passage = false;
                    Miss.Date_passage = "--/--/----";
                    Miss.ID_Formateur = null;
                    Miss.Remarque = "";
                }
                db.SaveChanges();
                var MissComp = db.Missions.Where(s => s.ID_Parcours == Miss.ID_Parcours).Where(s=>s.Passage==false).FirstOrDefault();
                if(MissComp == null)
                {
                    Parcours parc = db.Parcours.Find(Miss.ID_Parcours);
                    parc.Complété = true;
                }
                else
                {
                    Parcours parc = db.Parcours.Find(Miss.ID_Parcours);
                    parc.Complété = false;
                }
                db.SaveChanges();
            }
            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}
