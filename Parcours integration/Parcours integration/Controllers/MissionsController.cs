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
    public class MissionsController : BaseController
    {
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();

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
                       select m;

            var misspré = db.Missions.Where(s => s.ID_Parcours == parc.ID);

            foreach (var miss in misspré)
            {
                foreach(var item in Mods)
                {
                    if (miss.Nom_Mission == item.Modele.Nom)
                    {
                        Mods = Mods.Where(s => s.Modele.Nom != item.Modele.Nom);
                    }
                }
            }
            ViewBag.ID = ID;
            ViewBag.Modeles = new SelectList(Mods, "ID","Modele.Nom");
            ViewBag.Login_Interlocuteur = new SelectList(db.Employes, "Login", "Nom");
            return View();
        }

        // POST: Missions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom_Mission,Nom_Secteur,Login_Interlocuteur,Date_passage,Passage,ID_Parcours")] Missions missions, int ID, int Modeles)
        {
            missions.ID_Parcours = ID;
            missions.Date_passage = "--/--/----";
            var choix = db.Modele.Find(Modeles);

            missions.Nom_Mission = choix.Nom;
            missions.Nom_Secteur = choix.Secteurs.Nom;

            if (ModelState.IsValid)
            {
                db.Missions.Add(missions);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Login_Interlocuteur = new SelectList(db.Employes, "Login", "Mail", missions.Login_Interlocuteur);
            ViewBag.ID_Parcours = new SelectList(db.Parcours, "ID", "Nom", missions.ID_Parcours);
            return View(missions);
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
            ViewBag.Login_Interlocuteur = new SelectList(db.Employes, "Login", "Nom", missions.Login_Interlocuteur);
            return View(missions);
        }

        // POST: Missions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom_Mission,Nom_Secteur,Login_Interlocuteur,Date_passage,Passage,ID_Parcours,Remarque")] Missions missions)
        {
            if (ModelState.IsValid)
            {
                db.Entry(missions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Parcours", new { id = missions.ID_Parcours });
            }
            ViewBag.Login_Interlocuteur = new SelectList(db.Employes, "Login", "Mail", missions.Login_Interlocuteur);
            ViewBag.ID_Parcours = new SelectList(db.Parcours, "ID", "Nom", missions.ID_Parcours);
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
                    var Date = DateTime.Now.Date;
                    var jour = Date.ToString().Substring(0, 2);
                    var mois = Date.ToString().Substring(3, 2);
                    var année = Date.ToString().Substring(6, 4);
                    Miss.Date_passage = jour + "/" + mois + "/" + année;
                    Miss.Login_Interlocuteur = UserSession.Login;
                }
                else
                {
                    Miss.Passage = false;
                    Miss.Date_passage = "--/--/----";
                    Miss.Login_Interlocuteur = null;
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
