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
    public class OrganiController : BaseController
    {
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();
        // GET: Organi
        public ActionResult Index()
        {
            var list = from m in db.Secteurs
                       orderby m.Nom
                       select m;

            return View(list);
        }

        public ActionResult Create()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            List<string> list = new List<string>();
            var types = from m in db.Plan
                        select m.Type_de_salle;
            list.AddRange(types.Distinct());

            ViewBag.Types = new SelectList(list);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Type_de_salle,Nom_de_salle")] Plan salle, HttpPostedFileBase postedFile)
        {
            if (postedFile == null)
            {
                return Content("Vous n'avez pas envoyé de fichiers!");
            }
            else
            {
                var name = salle.Type_de_salle + salle.Nom_de_salle;
                string folderName = @"~/Docs/Plan/";

                var ServerSavePath = Path.Combine(Server.MapPath(folderName), name + Path.GetExtension(postedFile.FileName));
                if (System.IO.File.Exists(ServerSavePath))
                {
                    System.IO.File.Delete(ServerSavePath);
                }
                postedFile.SaveAs(ServerSavePath);
                salle.Image = name + Path.GetExtension(postedFile.FileName);
            }
            if (ModelState.IsValid)
            {
                db.Plan.Add(salle);
                db.SaveChanges();
                return RedirectToAction("Carte");
            }
            return View();
        }

        public ActionResult Edit(int id)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            Plan plan = db.Plan.Find(id);
            if (plan == null)
            {
                return HttpNotFound();
            }
            List<string> list = new List<string>();
            var types = from m in db.Plan
                        select m.Type_de_salle;
            list.AddRange(types.Distinct());

            ViewBag.Types = new SelectList(list);
            return View(plan);
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,Type_de_salle,Nom_de_salle,Image")] Plan salle, HttpPostedFileBase postedFile)
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            if (postedFile != null)
            {
                var name = salle.Type_de_salle + salle.Nom_de_salle;
                string folderName = @"~/Docs/Plan/";

                var ServerSavePath = Path.Combine(Server.MapPath(folderName), name + Path.GetExtension(postedFile.FileName));
                if (System.IO.File.Exists(ServerSavePath))
                {
                    System.IO.File.Delete(ServerSavePath);
                }
                postedFile.SaveAs(ServerSavePath);
                salle.Image = name + Path.GetExtension(postedFile.FileName);
            }
            if (ModelState.IsValid)
            {
                db.Entry(salle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Carte");
            }

            return View(salle);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plan plan = db.Plan.Find(id);
            if (plan == null)
            {
                return HttpNotFound();
            }
            return View(plan);
        }

        // POST: Employes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            Plan plan = db.Plan.Find(id);
            string folderName = @"~/Docs/Plan/";

            var ServerSavePath = Path.Combine(Server.MapPath(folderName),plan.Image);
            if (System.IO.File.Exists(ServerSavePath))
            {
                System.IO.File.Delete(ServerSavePath);
            }
            db.Plan.Remove(plan);
            db.SaveChanges();
            return RedirectToAction("Carte");
        }

        public ActionResult Carte()
        {
            List<string> TypeRoom = new List<string>();
            var list = from m in db.Plan
                       select m.Type_de_salle;
            TypeRoom.AddRange(list.Distinct());

            var Salles = from m in db.Plan
                         select m;

            ViewBag.Types = TypeRoom;
            return View(Salles);
        }
    }
}