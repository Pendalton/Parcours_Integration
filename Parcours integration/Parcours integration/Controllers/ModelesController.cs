using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Parcours_integration.Models;
using PagedList;
using PagedList.Mvc;


namespace Parcours_integration.Controllers
{
    public class ModelesController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        // GET: Modeles
        public ActionResult Index(int? ServiceID, bool CDI = true, bool CDD = true, bool Stage = true, bool Mutation = true, bool Intérim = true)
        {
            var Modeles = from m in db.Modele
                          select m;

            ViewBag.ServiceID = new SelectList(db.Service.Where(s => s.Actif), "ID", "Nom");
            if (ServiceID != null)
            {
                Modeles = Modeles.Where(d => d.Service.ID == ServiceID);
            }

            List<Modele> Résultat = new List<Modele>();
            Résultat.AddRange(Modeles);

            if (!CDI)
            {
                Résultat.RemoveAll(s => s.CDI);
            }
            if (!CDD)
            {
                Résultat.RemoveAll(s => s.CDD);
            }
            if (!Stage)
            {
                Résultat.RemoveAll(s => s.Stage);
            }
            if (!Mutation)
            {
                Résultat.RemoveAll(s => s.Mutation);
            }
            if (!Intérim)
            {
                Résultat.RemoveAll(s => s.Intérimaire);
            }

            ViewBag.CDI = CDI;
            ViewBag.CDD = CDD;
            ViewBag.Stage = Stage;
            ViewBag.Mutation = Mutation;
            ViewBag.Intérim = Intérim;
            ViewBag.SectSelec = ServiceID;

            return View(Résultat);
        }

        // GET: Modeles/Create
        public ActionResult Create()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            ViewBag.ServiceID = new SelectList(db.Service.Where(s => s.Actif == true), "ID","Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom,ServiceID,CDI,CDD,Stage,Mutation,Intérimaire")] Modele modele)
        {
            if (ModelState.IsValid)
            {
                if (modele.CDI)
                {
                    ModeleContrat MC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 1
                    };
                    db.ModeleContrat.Add(MC);
                }
                if (modele.CDD)
                {
                    ModeleContrat MC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 2
                    };
                    db.ModeleContrat.Add(MC);
                }
                if (modele.Stage)
                {
                    ModeleContrat MC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 3
                    };
                    db.ModeleContrat.Add(MC);
                }
                if (modele.Mutation)
                {
                    ModeleContrat MC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 4
                    };
                    db.ModeleContrat.Add(MC);
                }
                if (modele.Intérimaire)
                {
                    ModeleContrat MC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 7
                    };
                    db.ModeleContrat.Add(MC);
                }

                db.Modele.Add(modele);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ServiceID = new SelectList(db.Service.Where(s => s.Actif == true), "ID", "Nom");
            return View(modele);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Modele modele = db.Modele.Find(id);
            if (modele == null)
            {
                return HttpNotFound();
            }
            ViewBag.ServiceID = new SelectList(db.Service.Where(s => s.Actif == true), "ID", "Nom", modele.ServiceID);
            return View(modele);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom,ServiceID,CDI,CDD,Stage,Mutation,Intérimaire")] Modele modele)
        {
            var MC = db.ModeleContrat.Where(s=>s.ID_Modele == modele.ID).ToList();

            var IDPossibles = db.Contrat.Select(s => s.ID).ToList();

            foreach(var item in MC)
            {
                if (!modele.CDI && item.ID_Contrat == 1)
                {
                    db.ModeleContrat.Remove(item);
                    continue;
                }
                if (!modele.CDD && item.ID_Contrat == 2)
                {
                    db.ModeleContrat.Remove(item);
                    continue;
                }
                if (!modele.Stage && item.ID_Contrat == 3)
                {
                    db.ModeleContrat.Remove(item);
                    continue;
                }
                if (!modele.Mutation && item.ID_Contrat == 4)
                {
                    db.ModeleContrat.Remove(item);
                    continue;
                }
                if (!modele.Intérimaire && item.ID_Contrat == 7)
                {
                    db.ModeleContrat.Remove(item);
                    continue;
                }
            }

            if (MC.Where(s => s.ID_Contrat == 1).FirstOrDefault() == null && modele.CDI)
            {
                ModeleContrat MaC = new ModeleContrat
                {
                    ID_Modele = modele.ID,
                    ID_Contrat = 1
                };
                db.ModeleContrat.Add(MaC);
            }
            if (MC.Where(s => s.ID_Contrat == 2).FirstOrDefault() == null && modele.CDD)
            {
                ModeleContrat MaC = new ModeleContrat
                {
                    ID_Modele = modele.ID,
                    ID_Contrat = 2
                };
                db.ModeleContrat.Add(MaC);
            }
            if (MC.Where(s => s.ID_Contrat == 3).FirstOrDefault() == null && modele.Stage)
            {
                ModeleContrat MaC = new ModeleContrat
                {
                    ID_Modele = modele.ID,
                    ID_Contrat = 3
                };
                db.ModeleContrat.Add(MaC);
            }
            if (MC.Where(s => s.ID_Contrat == 4).FirstOrDefault() == null && modele.Mutation)
            {
                ModeleContrat MaC = new ModeleContrat
                {
                    ID_Modele = modele.ID,
                    ID_Contrat = 4
                };
                db.ModeleContrat.Add(MaC);
            }
            if (MC.Where(s => s.ID_Contrat == 7).FirstOrDefault() == null && modele.Intérimaire)
            {
                ModeleContrat MaC = new ModeleContrat
                {
                    ID_Modele = modele.ID,
                    ID_Contrat = 7
                };
                db.ModeleContrat.Add(MaC);
            }

            if (ModelState.IsValid)
            {
                db.Entry(modele).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ServiceID = new SelectList(db.Service.Where(s => s.Actif==true), "ID", "Nom", modele.ServiceID);
            return View(modele);
        }

        // GET: Modeles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Modele modele = db.Modele.Find(id);
            if (modele == null)
            {
                return HttpNotFound();
            }
            return View(modele);
        }

        // POST: Modeles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Modele modele = db.Modele.Find(id);
            var MC = db.ModeleContrat.Where(s => s.ID_Modele == id);
            foreach(var item in MC)
            {
                db.ModeleContrat.Remove(item);
            }
            db.Modele.Remove(modele);
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
