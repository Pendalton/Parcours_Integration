using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Parcours_integration.Models;
using Newtonsoft.Json;



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

        public bool ChangeCDI(int ID)
        {
            var modele = db.Modele.Find(ID);
            var MC_CDI = db.ModeleContrat.Where(s => s.ID_Modele == ID).Where(s => s.ID_Contrat == 1).FirstOrDefault();

            if(!modele.CDI)
            {
                if (MC_CDI == null)
                {
                    ModeleContrat MaC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 1
                    };
                    db.ModeleContrat.Add(MaC);
                }
            }
            else
            {
                if (MC_CDI != null)
                {
                    db.ModeleContrat.Remove(MC_CDI);
                }
            }

            modele.CDI = !modele.CDI;

            if (ModelState.IsValid)
            {
                db.Entry(modele).State = EntityState.Modified;
                db.SaveChanges();

                return modele.CDI;
            }
            return modele.CDI;
        }

        public bool ChangeCDD(int ID)
        {
            var modele = db.Modele.Find(ID);
            var MC_CDD = db.ModeleContrat.Where(s => s.ID_Modele == ID).Where(s => s.ID_Contrat == 2).FirstOrDefault();

            if (!modele.CDD)
            {
                if (MC_CDD == null)
                {
                    ModeleContrat MaC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 2
                    };
                    db.ModeleContrat.Add(MaC);
                }
            }
            else
            {
                if (MC_CDD != null)
                {
                    db.ModeleContrat.Remove(MC_CDD);
                }
            }

            modele.CDD = !modele.CDD;

            if (ModelState.IsValid)
            {
                db.Entry(modele).State = EntityState.Modified;
                db.SaveChanges();

                return modele.CDD;
            }
            return modele.CDD;
        }

        public bool ChangeStage(int ID)
        {
            var modele = db.Modele.Find(ID);
            var MC_Stage = db.ModeleContrat.Where(s => s.ID_Modele == ID).Where(s => s.ID_Contrat == 3).FirstOrDefault();

            if (!modele.Stage)
            {
                if (MC_Stage == null)
                {
                    ModeleContrat MaC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 3
                    };
                    db.ModeleContrat.Add(MaC);
                }
            }
            else
            {
                if (MC_Stage != null)
                {
                    db.ModeleContrat.Remove(MC_Stage);
                }
            }

            modele.Stage = !modele.Stage;

            if (ModelState.IsValid)
            {
                db.Entry(modele).State = EntityState.Modified;
                db.SaveChanges();

                return modele.Stage;
            }
            return modele.Stage;
        }

        public bool ChangeMutation(int ID)
        {
            var modele = db.Modele.Find(ID);
            var MC_Mutation = db.ModeleContrat.Where(s => s.ID_Modele == ID).Where(s => s.ID_Contrat == 4).FirstOrDefault();

            if (!modele.CDI)
            {
                if (MC_Mutation == null)
                {
                    ModeleContrat MaC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 4
                    };
                    db.ModeleContrat.Add(MaC);
                }
            }
            else
            {
                if (MC_Mutation != null)
                {
                    db.ModeleContrat.Remove(MC_Mutation);
                }
            }

            modele.Mutation = !modele.Mutation;

            if (ModelState.IsValid)
            {
                db.Entry(modele).State = EntityState.Modified;
                db.SaveChanges();

                return modele.Mutation;
            }
            return modele.Mutation;
        }

        public bool ChangeIntérimaire(int ID)
        {
            var modele = db.Modele.Find(ID);
            var MC_Intérim = db.ModeleContrat.Where(s => s.ID_Modele == ID).Where(s => s.ID_Contrat == 7).FirstOrDefault();

            if (!modele.CDI)
            {
                if (MC_Intérim == null)
                {
                    ModeleContrat MaC = new ModeleContrat
                    {
                        ID_Modele = modele.ID,
                        ID_Contrat = 7
                    };
                    db.ModeleContrat.Add(MaC);
                }
            }
            else
            {
                if (MC_Intérim != null)
                {
                    db.ModeleContrat.Remove(MC_Intérim);
                }
            }

            modele.Intérimaire = !modele.Intérimaire;

            if (ModelState.IsValid)
            {
                db.Entry(modele).State = EntityState.Modified;
                db.SaveChanges();

                return modele.Intérimaire;
            }
            return modele.Intérimaire;
        }

        public string Update(int ID, string Name, int ServID)
        {
            var Modele = db.Modele.Find(ID);

            if (Modele != null)
            {
                Modele.Nom = Name;
                Modele.ServiceID = ServID;
            }
            db.Entry(Modele).State = EntityState.Modified;
            db.SaveChanges();
            var Données = Tuple.Create(Modele.Nom, Modele.Service.Nom);

            string json = JsonConvert.SerializeObject(Données);

            return json;
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
