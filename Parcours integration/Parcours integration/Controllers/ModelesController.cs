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
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();

        // GET: Modeles
        public ActionResult Index(string Secteurs, bool? CDI=false, bool? CDD=false, bool? Stage=false, bool? Mutation=false)
        {
            var Modeles = from m in db.Modele
                          select m;

            var SecteurList = new List<string>();
            var SecteurQry = from m in db.Secteurs
                             orderby m.Nom
                             select m.Nom;
            SecteurList.AddRange(SecteurQry.Distinct());
            ViewBag.Secteurs = new SelectList(SecteurList);
            if (!String.IsNullOrEmpty(Secteurs))
            {
                Modeles = Modeles.Where(d => d.Secteurs.Nom == Secteurs);
            }

            if (CDI == true)
            {
                Modeles = Modeles.Where(s => s.CDI == true);
            }
            if (CDD == true)
            {
                Modeles = Modeles.Where(s => s.CDD == true);
            }
            if (Stage == true)
            {
                Modeles = Modeles.Where(s => s.Stage == true);
            }
            if (Mutation == true)
            {
                Modeles = Modeles.Where(s => s.Mutation == true);
            }


            ViewBag.CDI = CDI;
            ViewBag.CDD = CDD;
            ViewBag.Stage = Stage;
            ViewBag.Mutat = Mutation;
            ViewBag.SectSelec = Secteurs;
            return View(Modeles);
        }

        // GET: Modeles/Create
        public ActionResult Create()
        {
            if (!EstAdmin)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom","Nom");
            return View();
        }

        // POST: Modeles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom,Secteur,CDI,CDD,Stage,Mutation")] Modele modele)
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

                db.Modele.Add(modele);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom");
            return View(modele);
        }

        // GET: Modeles/Edit/5
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
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif == true), "Nom", "Nom", modele.Secteur);
            return View(modele);
        }

        // POST: Modeles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom,Secteur,CDI,CDD,Stage,Mutation")] Modele modele)
        {
            var MC = db.ModeleContrat.Where(s=>s.ID_Modele == modele.ID);

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

            if (ModelState.IsValid)
            {
                db.Entry(modele).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Secteur = new SelectList(db.Secteurs.Where(s => s.Actif==true), "Nom", "Nom",modele.Secteur);
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
