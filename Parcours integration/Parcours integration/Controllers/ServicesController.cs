using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;
using Parcours_integration.Models;
using System.ComponentModel.DataAnnotations;

namespace Parcours_integration.Controllers
{
    public class ServicesController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        // GET: Services
        public ActionResult Index()
        {
            dynamic Ressources = new ExpandoObject();

            var Serv = db.Service.OrderBy(s => s.Nom).ToList();
            var Cont = db.Contrat.OrderBy(s => s.Nom).ToList();
            var Equ = db.Equipe.OrderBy(s => s.Nom).ToList();


            Ressources.Services = Serv.OrderByDescending(s => s.Actif);
            Ressources.Contrats = Cont.OrderByDescending(s => s.Actif);
            Ressources.Equipes = Equ.OrderByDescending(s => s.Actif);

            return View(Ressources);
        }

        //############################################# Partie Services ###########################################

        public ActionResult Create()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom,Actif")] Service service)
        {
            if (ModelState.IsValid)
            {
                db.Service.Add(service);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(service);
        }

        // GET: Services/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Service service = db.Service.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom,Actif")] Service service)
        {
            if (ModelState.IsValid)
            {
                db.Entry(service).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(service);
        }

        public ActionResult Desactiver(int? id)
        {
            Service sect = db.Service.Find(id);
            sect.Actif = !sect.Actif;
            db.SaveChanges();
            return new EmptyResult();
        }

        //############################################# Partie Contrats ###########################################

        public ActionResult CreateCont()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCont([Bind(Include = "ID,Nom")] Contrat contrat)
        {
            if (ModelState.IsValid)
            {
                db.Contrat.Add(contrat);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(contrat);
        }

        public ActionResult EditCont(int? id)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contrat contrat = db.Contrat.Find(id);
            if (contrat == null)
            {
                return HttpNotFound();
            }
            return View(contrat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCont([Bind(Include = "ID,Nom")] Contrat contrat)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contrat).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contrat);
        }

        public ActionResult DesactiverCont(int? id)
        {
            Contrat contrat = db.Contrat.Find(id);
            contrat.Actif = !contrat.Actif;
            db.SaveChanges();
            return new EmptyResult();
        }

        //############################################# Partie Equipes ###########################################

        public ActionResult CreateEqu()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEqu([Bind(Include = "ID,Nom")] Equipe equipe)
        {
            if (ModelState.IsValid)
            {
                db.Equipe.Add(equipe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(equipe);
        }

        [HttpPost]
        public JsonResult InsertEqu(Equipe equipe)
        {
            using (db)
            {
                db.Equipe.Add(equipe);
                db.SaveChanges();
            }

            return Json(equipe);
        }

        [HttpPost]
        public ActionResult UpdateEqu(Equipe equipe)
        {
            using (db)
            {
                Equipe UpdatedEqu = db.Equipe.Where(s => s.ID == equipe.ID).FirstOrDefault();
                UpdatedEqu.Nom = equipe.Nom;
                UpdatedEqu.Actif = equipe.Actif;
                db.SaveChanges();
            }
            return new EmptyResult();
        }


        public ActionResult EditEqu(int? id)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipe equipe = db.Equipe.Find(id);
            if (equipe == null)
            {
                return HttpNotFound();
            }
            return View(equipe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEqu([Bind(Include = "ID,Nom")] Equipe equipe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(equipe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(equipe);
        }

        public ActionResult DesactiverEqu(int? id)
        {
            Equipe equipe = db.Equipe.Find(id);
            equipe.Actif = !equipe.Actif;
            db.SaveChanges();
            return new EmptyResult();
        }
        //######################################################################################################################

        public ActionResult Delete(int? id)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Service service = db.Service.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Service service = db.Service.Find(id);
            db.Service.Remove(service);
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
