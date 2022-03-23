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
    public class ParcoursController : BaseController
    {
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();

        // GET: Parcours
        public ActionResult Index(bool? CDI, bool? CDD, bool? Stage, bool? Mutation, bool Terminé = false)
        {
            var parcour = from p in db.Parcours
                          select p;

            if (Terminé)
            {
                parcour = parcour.Where(s => s.Complété==true|| s.Complété == false);
            }
            else
            {
                parcour = parcour.Where(s => s.Complété == false);
            }

            if (CDI == true)
            {
                parcour = parcour.Where(s => s.Type_Contrat == 1);
            }
            if (CDD == true)
            {
                parcour = parcour.Where(s => s.Type_Contrat == 2);
            }
            if (Stage == true)
            {
                parcour = parcour.Where(s => s.Type_Contrat == 3);
            }
            if (Mutation == true)
            {
                parcour = parcour.Where(s => s.Type_Contrat == 4);
            }

            foreach(var item in parcour)
            {
                var jour = item.Date_entrée.Substring(8, 2);
                var mois = item.Date_entrée.Substring(5, 2);
                var année = item.Date_entrée.Substring(0, 4);
                item.Date_entrée = jour + "/" + mois + "/" + année;
            }

            ViewBag.CDI = CDI;
            ViewBag.CDD = CDD;
            ViewBag.Stage = Stage;
            ViewBag.Mutat = Mutation;
            ViewBag.Term = Terminé;

            return View(parcour);
        }

        // GET: Parcours/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Parcours parcours = db.Parcours.Find(id);
            if (parcours == null)
            {
                return RedirectToAction("Index");
            }

            var jour = parcours.Date_entrée.Substring(8, 2);
            var mois = parcours.Date_entrée.Substring(5, 2);
            var année = parcours.Date_entrée.Substring(0, 4);
            parcours.Date_entrée = jour + "/" + mois + "/" + année;

            return View(parcours);
        }

        // GET: Parcours/Create
        public ActionResult Create()
        {
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom");
            return View();
        }

        // POST: Parcours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom,Prénom,Date_entrée,Type_Contrat,Poste")] Parcours parcours)
        {
            var Date = DateTime.Now.Date;

            var jour = Date.ToString().Substring(0, 2);
            var mois = Date.ToString().Substring(3, 2);
            var année = Date.ToString().Substring(6, 4);
            parcours.Date_entrée = année + "-" + mois + "-" + jour;
            parcours.Complété = false;

            if (ModelState.IsValid)
            {
                db.Parcours.Add(parcours);
                var mods = db.ModeleContrat.Where(s => s.ID_Contrat == parcours.Type_Contrat);

                foreach (var item in mods)
                {
                    Missions miss = new Missions
                    {
                        ID_Parcours = parcours.ID,
                        Date_passage = "--/--/----",
                        Nom_Mission = item.Modele.Nom,
                        Nom_Secteur = item.Modele.Secteur,
                        Passage = false,
                    };

                    if (ModelState.IsValid)
                    {
                        db.Missions.Add(miss);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom");
            return View(parcours);
        }

        // GET: Parcours/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Parcours parcours = db.Parcours.Find(id);
            if (parcours == null)
            {
                return HttpNotFound();
            }
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom", parcours.Type_Contrat);
            return View(parcours);
        }

        // POST: Parcours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom,Prénom,Date_entrée,Type_Contrat,Poste,Complété")] Parcours parcours)
        {
            if(parcours.Type_Contrat != null)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(parcours).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = parcours.ID });
                }
            }
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom", parcours.Type_Contrat);
            return View(parcours);
        }

        // GET: Parcours/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Parcours parcours = db.Parcours.Find(id);
            if (parcours == null)
            {
                return HttpNotFound();
            }

            var jour = parcours.Date_entrée.Substring(8, 2);
            var mois = parcours.Date_entrée.Substring(5, 2);
            var année = parcours.Date_entrée.Substring(0, 4);
            parcours.Date_entrée = jour + "/" + mois + "/" + année;

            return View(parcours);
        }

        // POST: Parcours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Parcours parcours = db.Parcours.Find(id);
            var miss = db.Missions.Where(s => s.ID_Parcours == id);
            foreach(var item in miss)
            {
                db.Missions.Remove(item);
            }
            db.Parcours.Remove(parcours);
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
    }
}
