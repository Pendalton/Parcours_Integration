using System;
using System.IO;
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
    public class EmployesController : BaseController
    {
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();

        // GET: Employes
        public ActionResult Index(string secteur)
        {
            var employes = from m in db.Employes
                           select m;

            var SectList = new List<string>();
            var sect = from m in db.Secteurs
                       select m.Nom;
            SectList.AddRange(sect);
            ViewBag.secteur = new SelectList(SectList);
            if (!String.IsNullOrEmpty(secteur))
            {
                employes = employes.Where(s => s.Secteur == secteur);
            }

            return View(employes);
        }

        // GET: Employes/Create
        public ActionResult Create()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom");
            ViewBag.Reporting = new SelectList(db.Employes, "Login", "Nom");
            return View();
        }

        // POST: Employes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Login,Intervenant,Mail,Secteur,Nom,Reporting")] Employes employes, HttpPostedFileBase postedFile)
        {
            if (!employes.Login.Contains("CORPORATE\\"))
            {
                employes.Login = "CORPORATE\\" + employes.Login;
            }
            if(employes.Nom == null ||employes.Mail == null)
            {
                ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom");
                return View(employes);
            }

            if (postedFile != null && postedFile.ContentLength > 0)
            {
                byte[] bytes;
                using(BinaryReader br = new BinaryReader(postedFile.InputStream))
                {
                    bytes = br.ReadBytes(postedFile.ContentLength);
                }
                employes.Photo = bytes;
            }

            if (ModelState.IsValid)
            {
                db.Employes.Add(employes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom");
            ViewBag.Reporting = new SelectList(db.Employes, "Login", "Nom");
            return View(employes);
        }

        // GET: Employes/Edit/5
        public ActionResult Edit(string Login)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (Login == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employes employes = db.Employes.Find(Login);
            if (employes == null)
            {
                return HttpNotFound();
            }
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom", employes.Secteur);
            ViewBag.Reporting = new SelectList(db.Employes.Where(s=>s.Login != employes.Login), "Login", "Nom", employes.Reporting);
            return View(employes);
        }

        // POST: Employes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Login,Intervenant,Mail,Secteur,Nom,Photo,Reporting")] Employes employes, HttpPostedFileBase postedFile)
        {
            if (employes.Nom == null || employes.Mail == null)
            {
                ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom");
                return View(employes);
            }

            if (postedFile != null && postedFile.ContentLength > 0)
            {
                byte[] bytes;
                using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                {
                    bytes = br.ReadBytes(postedFile.ContentLength);
                }
                employes.Photo = bytes;
            }

            if (ModelState.IsValid)
            {
                db.Entry(employes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom", employes.Secteur);
            ViewBag.Reporting = new SelectList(db.Employes.Where(s => s.Login != employes.Login), "Login", "Nom", employes.Reporting);
            return View(employes);
        }

        // GET: Employes/Delete/5
        public ActionResult Delete(string login)
        {
            if (login == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employes employes = db.Employes.Find(login);
            if (employes == null)
            {
                return HttpNotFound();
            }
            return View(employes);
        }

        // POST: Employes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string login)
        {
            Employes employes = db.Employes.Find(login);
            db.Employes.Remove(employes);
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
