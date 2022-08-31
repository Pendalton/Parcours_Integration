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
    public class UtilisateursController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        // GET: Employes
        public ActionResult Index(int? Service_ID)
        {
            var users = from m in db.Utilisateurs
                        where m.EstFormateur == true
                        select m;

            ViewBag.Service_ID = new SelectList(db.Service.ToList(), "ID", "Nom");
            if (Service_ID != null)
            {
                var Service = db.Utilisateurs_Services.Where(s => s.ID_Service == Service_ID).Select(s => s.ID_Utilisateur);

                users = users.Where(s => Service.Contains(s.ID));
            }
            
            var Services = db.Utilisateurs_Services.ToList();

            List<Tuple<int, int, int, string>> ListServ = new List<Tuple<int, int, int, string>>();

            foreach (var item in Services) 
            {
                ListServ.Add(new Tuple<int, int, int, string>(item.ID, item.ID_Service, item.ID_Utilisateur, item.Service.Nom));
            }

            ViewBag.Service = ListServ;

            return View(users);
        }

        // GET: Employes/Create
        public ActionResult Create()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Service = new MultiSelectList(db.Service.Where(s => s.Actif).Where(s => s.ID != 7), "ID", "Nom");
            ViewBag.User = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom).Where(s=>!s.EstFormateur), "ID", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase postedFile, int[] Service, int User, bool EstAdmin)
        {
            Utilisateurs employes = db.Utilisateurs.Find(User);

            employes.EstFormateur = true;
            employes.EstAdmin = EstAdmin;

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

                foreach (var value in Service)
                {
                    Utilisateurs_Services EmpSer = new Utilisateurs_Services
                    {
                        ID_Utilisateur = employes.ID,
                        ID_Service = value
                    };
                    db.Utilisateurs_Services.Add(EmpSer);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Service = new MultiSelectList(db.Service.Where(s => s.Actif).Where(s => s.ID != 7), "ID", "Nom");
            ViewBag.User = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom).Where(s => !s.EstFormateur), "ID", "Nom");
            return View(employes);
        }

        // GET: Employes/Edit/5
        public ActionResult Edit(int? ID)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utilisateurs employes = db.Utilisateurs.Find(ID);
            if (employes == null)
            {
                return HttpNotFound();
            }
            var ump = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == employes.ID).Select(s => s.ID_Service).ToArray();
            ViewBag.Service = new MultiSelectList(db.Service.Where(s => s.Actif).Where(s=>s.ID != 7), "ID", "Nom", ump);
            return View(employes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom,Login,UserMail,EstResponsable,EstFormateur,EstAdmin")] Utilisateurs employes, HttpPostedFileBase postedFile, int[] Service)
        {
            var ump = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == employes.ID).Select(s => s.ID).ToArray();
            if (employes.Nom == null || employes.UserMail == null)
            {
                ViewBag.Service = new MultiSelectList(db.Service.Where(s => s.Actif).Where(s => s.ID != 7), "ID", "Nom", ump);
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
                var Exists = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == employes.ID).ToList();

                foreach (var value in Service)
                {
                    if (!Exists.Contains(Exists.Where(s => s.ID_Service == value).FirstOrDefault()))
                    {
                        Utilisateurs_Services EmpSer = new Utilisateurs_Services
                        {
                            ID_Utilisateur = employes.ID,
                            ID_Service = value
                        };
                        db.Utilisateurs_Services.Add(EmpSer);
                    }
                }

                foreach (var SEREMP in Exists)
                {
                    if (!Service.Contains(SEREMP.ID_Service))
                    {
                        db.Utilisateurs_Services.Remove(SEREMP);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Service = new MultiSelectList(db.Service.Where(s => s.Actif).Where(s => s.ID != 7), "ID", "Nom", ump);
            return View(employes);
        }

        // GET: Employes/Delete/5
        public ActionResult Delete(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Utilisateurs employes = db.Utilisateurs.Find(ID);
            if (employes == null)
            {
                return HttpNotFound();
            }
            ViewBag.Services = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == ID).ToList();
            return View(employes);
        }

        // POST: Employes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? ID)
        {
            Utilisateurs employes = db.Utilisateurs.Find(ID);

            var Exists = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == ID).ToList();

            db.Utilisateurs_Services.RemoveRange(Exists);

            employes.EstFormateur = false;
            employes.EstAdmin = false;
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
