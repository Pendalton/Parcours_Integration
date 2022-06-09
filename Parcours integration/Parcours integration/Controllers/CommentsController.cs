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
    public class CommentsController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(FormCollection form)
        {
            var text = form["CommentText"].ToString();
            var ParcoursID = int.Parse(form["ParcoursID"]);
            var rating = int.Parse(form["Rating"]);

            var Date = DateTime.Now.Date;
            var jour = Date.ToString().Substring(0, 2);
            var mois = Date.ToString().Substring(3, 2);
            var année = Date.ToString().Substring(6, 4);

            Comment comm = db.Comment.Where(s => s.ParcoursID == ParcoursID).FirstOrDefault();
            if(comm!= null)
            {
                comm.ParcoursID = ParcoursID;
                comm.CommentText = text;
                comm.Rating = rating;
                comm.DateHeure = année + "-" + mois + "-" + jour;

                db.Entry(comm).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                Comment artComment = new Comment()
                {
                    ParcoursID = ParcoursID,
                    CommentText = text,
                    Rating = rating,
                    DateHeure = année + "-" + mois + "-" + jour,
                };

                db.Comment.Add(artComment);
                db.SaveChanges();
            }
            return RedirectToAction("Details", "Parcours", new { id = ParcoursID });
        }

        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comment.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comment.Find(id);
            db.Comment.Remove(comment);
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
