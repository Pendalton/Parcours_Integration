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
    public class SecteursController : BaseController
    {
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();

        // GET: Secteurs
        public ActionResult Index()
        {
            var Sects = from m in db.Secteurs
                        orderby m.Nom
                        select m;

            return View(Sects);
        }

        // GET: Secteurs/Create
        public ActionResult Create()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // POST: Secteurs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nom,Actif")] Secteurs secteurs)
        {
            if (ModelState.IsValid)
            {
                db.Secteurs.Add(secteurs);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(secteurs);
        }

        // GET: Secteurs/Edit/5
        public ActionResult Edit(string Nom)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (Nom == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Secteurs secteurs = db.Secteurs.Find(Nom);
            if (secteurs == null)
            {
                return HttpNotFound();
            }
            return View(secteurs);
        }

        public ActionResult Desactiver(string Nom)
        {
            Secteurs sect = db.Secteurs.Find(Nom);
            if(sect.Actif == false)
            {
                sect.Actif = true;
            }
            else
            {
                sect.Actif = false;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Secteurs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Nom,Actif")] Secteurs secteurs)
        {
            if (ModelState.IsValid)
            {
                db.Entry(secteurs).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(secteurs);
        }

        // GET: Secteurs/Delete/5
        public ActionResult Delete(string Nom)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (Nom == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Secteurs secteurs = db.Secteurs.Find(Nom);
            if (secteurs == null)
            {
                return HttpNotFound();
            }
            return View(secteurs);
        }

        // POST: Secteurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Nom)
        {
            Secteurs secteurs = db.Secteurs.Find(Nom);
            db.Secteurs.Remove(secteurs);
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
