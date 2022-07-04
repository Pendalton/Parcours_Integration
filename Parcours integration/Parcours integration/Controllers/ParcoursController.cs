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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Net.Mail;
using iTextSharp.tool.xml;
using PagedList;

namespace Parcours_integration.Controllers
{
    public class ParcoursController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        // GET: Parcours
        public ActionResult Index(int? page, string Datepicker, string Datepicker2, int? Rythme, bool CDI = true, bool CDD = true, bool Stage = true, bool Mutation = true, bool Terminé = false, bool Intérim = true)
        {
            var parcour = from p in db.Parcours
                          select p;
            DateTime Picked = DateTime.MinValue;
            DateTime Picked1 = DateTime.MinValue;

            List<Parcours> Résultat = new List<Parcours>();

            if (Terminé)
            {
                parcour = db.Parcours.Where(s => s.Complété==true|| s.Complété == false);
            }
            else
            {
                parcour = db.Parcours.Where(s => s.Complété == false);
            }

            ViewBag.Rythme = new SelectList(db.Equipe, "ID", "Nom");

            if (Rythme != null)
            {
                parcour = parcour.Where(s => s.EquipeID == Rythme);
            }

            if (String.IsNullOrEmpty(Datepicker2))
            {
                if (!String.IsNullOrEmpty(Datepicker))
                {
                    Picked = Convert.ToDateTime(Datepicker).Date;
                    parcour = parcour.Where(s => s.Entrée >= Picked);
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(Datepicker))
                {
                    Picked = Convert.ToDateTime(Datepicker).Date;
                    Picked1 = Convert.ToDateTime(Datepicker2).Date;
                    parcour = parcour.Where(s => s.Entrée >= Picked && s.Entrée < Picked1);
                }
            }

            if (CDI)
            {
                Résultat.AddRange(parcour.Where(s => s.Type_Contrat == 1));
            }
            if (CDD)
            {
                Résultat.AddRange(parcour.Where(s => s.Type_Contrat == 2));
            }
            if (Stage)
            {
                Résultat.AddRange(parcour.Where(s => s.Type_Contrat == 3));
            }
            if (Mutation)
            {
                Résultat.AddRange(parcour.Where(s => s.Type_Contrat == 4));
            }
            if (Intérim)
            {
                Résultat.AddRange(parcour.Where(s => s.Type_Contrat == 7));
            }

            foreach (var item in Résultat)
            {
                item.Date_entrée = item.Entrée.ToString().Substring(0, 10);
            }

            ViewBag.CDI = CDI;
            ViewBag.CDD = CDD;
            ViewBag.Stage = Stage;
            ViewBag.Mutation = Mutation;
            ViewBag.Intérim = Intérim;
            ViewBag.Term = Terminé;
            ViewBag.Datepicker = Datepicker;
            ViewBag.Datepicker2 = Datepicker2;

            Résultat = Résultat.OrderByDescending(s=>s.ID).ToList();

            return View(Résultat.ToList().ToPagedList(page?? 1,15));
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

            var comments = db.Comment.Where(s => s.ParcoursID == parcours.ID).FirstOrDefault();

            if (comments != null)
            {
                ViewBag.Commenté = true;
                ViewBag.Rating = comments.Rating;
                ViewBag.CommentText = comments.CommentText;
            }
            else
            {
                ViewBag.Commenté = false;
            }

            ViewBag.Comments = comments;
            ViewBag.ParcoursID = id.Value;
            ViewBag.Identity = parcours.Nom + " " + parcours.Prénom;
            ViewBag.TypeContrat = parcours.Contrat.Nom + " - " + parcours.Equipe.Nom;
            ViewBag.PosteOccupé = parcours.Poste;
            ViewBag.Entrée = parcours.Entrée.ToString().Substring(0,10);

            List<string> Lieux = new List<string>();
            var adding = from m in db.Utilisateurs_Services
                         select m.Service.Nom;
            Lieux.AddRange(adding.Distinct());
            ViewBag.Lieux = Lieux;

            ViewBag.Intervenants = db.Utilisateurs_Services.ToList();

            return View(parcours);
        }

        // GET: Parcours/Create
        public ActionResult Create()
        {
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom");
            ViewBag.EquipeID = new SelectList(db.Equipe, "ID", "Nom");
            ViewBag.ID_Resp = new SelectList(db.Utilisateurs.OrderBy(s=>s.Nom), "ID", "Nom");
            ViewBag.ID_Employé = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom,Prénom,Date_entrée,Type_Contrat,Poste,EquipeID,ID_Resp,ID_Employé")] Parcours parcours)
        {
            var Date = DateTime.Now.Date;

            parcours.Entrée = Date;

            parcours.Complété = false;

            if (ModelState.IsValid)
            {
                db.Parcours.Add(parcours);
                var mods = db.ModeleContrat.Where(s => s.ID_Contrat == parcours.Type_Contrat).ToArray();
                Utilisateurs resp = db.Utilisateurs.Find(parcours.ID_Resp);

                resp.EstResponsable = true;
                db.Entry(resp).State = EntityState.Modified;
                foreach (var item in mods)
                {
                    Missions miss = new Missions
                    {
                        ID_Parcours = parcours.ID,
                        Date_passage = "--/--/----",
                        Nom_Mission = item.Modele.Nom,
                        Nom_Secteur = item.Modele.Service.Nom,
                        Passage = false,
                    };

                    if (ModelState.IsValid)
                    {
                        db.Missions.Add(miss);
                    }
                }
                db.SaveChanges();
                string name = parcours.ID + parcours.Nom + parcours.Prénom;
                string folderName = @"~/Docs/" + name;
                if (!Directory.Exists(Server.MapPath(folderName)))
                {
                    Directory.CreateDirectory(Server.MapPath(folderName));
                }
                return RedirectToAction("Index");
            }
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom");
            ViewBag.EquipeID = new SelectList(db.Equipe, "ID", "Nom");
            ViewBag.ID_Resp = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom");
            ViewBag.ID_Employé = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom");
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

            ViewData["Dossier"] = parcours.ID + parcours.Nom + parcours.Prénom;
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom", parcours.Type_Contrat);
            ViewBag.EquipeID = new SelectList(db.Equipe, "ID", "Nom", parcours.EquipeID);
            ViewBag.ID_Resp = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom", parcours.ID_Resp);
            ViewBag.ID_Employé = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom", parcours.ID_Employé);
            ViewBag.Resp = parcours.ID_Resp;
            return View(parcours);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nom,Prénom,Date_entrée,Type_Contrat,Poste,Complété,Entrée,EquipeID,ID_Resp,ID_Employé")] Parcours parcours, string Dossier, int OldResp)
        {
            parcours.Entrée = Convert.ToDateTime(parcours.Date_entrée);

            if(parcours.Type_Contrat != null)
            {
                if (ModelState.IsValid)
                {
                    string folder = @"~/Docs/" + Dossier;
                    string newFold = @"~/Docs/" + parcours.ID + parcours.Nom + parcours.Prénom;
                    if(folder != newFold) 
                    {
                        if (Directory.Exists(Server.MapPath(folder)))
                        {
                            Directory.Move(Server.MapPath(folder), Server.MapPath(newFold));
                        }
                    }

                    if(parcours.ID_Resp != OldResp)
                    {
                        var resp = db.Utilisateurs.Find(parcours.ID_Resp);
                        resp.EstResponsable = true;
                        var oldres = db.Utilisateurs.Find(OldResp);
                        oldres.EstResponsable = false;
                    }

                    db.Entry(parcours).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = parcours.ID });
                }
            }
            ViewBag.Type_Contrat = new SelectList(db.Contrat, "ID", "Nom", parcours.Type_Contrat);
            ViewBag.EquipeID = new SelectList(db.Equipe, "ID", "Nom", parcours.EquipeID);
            ViewBag.ID_Resp = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom", parcours.ID_Resp);
            ViewBag.ID_Employé = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom", parcours.ID_Employé);
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

            parcours.Date_entrée = parcours.Entrée.ToString().Substring(0, 10);

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
            string name = parcours.ID + parcours.Nom + parcours.Prénom;
            string folderName = @"~/Docs/" + name;
            if (Directory.Exists(Server.MapPath(folderName)))
            {
                Directory.Delete(Server.MapPath(folderName),true);
            }
            db.Parcours.Remove(parcours);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AddFile(int id)
        {
            var parcours = db.Parcours.Find(id);
            if (parcours == null) 
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            var path = @"~/Docs/" + parcours.ID + parcours.Nom + parcours.Prénom;
            var ServerPath = Server.MapPath(path);
            if (!Directory.Exists(ServerPath))
            {
                Directory.CreateDirectory(ServerPath);
            }

            string[] files = Directory.GetFiles(ServerPath);
            List<FileModel> list = new List<FileModel>();
            foreach(string file in files)
            {
                list.Add(new FileModel { FileName = Path.GetFileName(file) });
            }

            ViewBag.Fichers = list;
            ViewBag.Parc = parcours.Nom + " " + parcours.Prénom;
            ViewData["id"] = parcours.ID;
            return View(parcours);
        }

        public FileResult DownloadFile(string fileName, int id)
        {
            Parcours parc = db.Parcours.Find(id);
            var Emp = parc.ID + parc.Nom + parc.Prénom;            

            string path = Path.Combine(Server.MapPath("~/Docs/"), Emp, fileName);
            var NomFichier = Path.GetFileNameWithoutExtension(path);
            var ExtFichier = Path.GetExtension(path);
            if (System.IO.File.Exists(path))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                return File(bytes, "application/octet-stream", NomFichier + "_" + parc.Nom + "_" + parc.Prénom + ExtFichier);
            }
            else
            {
                return null;
            }
        }

        public ActionResult DeleteFile(string fileName, int id)
        {
            Parcours parcours = db.Parcours.Find(id);
            var emp = id + parcours.Nom + parcours.Prénom;
            string fullPath = Path.Combine(Server.MapPath("~/Docs"), emp, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            return RedirectToAction("AddFile", new { id });
        }

        [HttpPost]
        public ActionResult AddFile(HttpPostedFileBase[] postedFiles, int id)
        {

            int i = 1;
            if(postedFiles == null || postedFiles.Length == 0)
            {
                return Content("Vous n'avez pas envoyé de fichiers!");
            }
            else
            {
                Parcours parcours = db.Parcours.Find(id);
                var name = parcours.ID + parcours.Nom + parcours.Prénom;
                string folderName = @"~/Docs/" + name;

                foreach(HttpPostedFileBase file in postedFiles)
                {
                    if(file != null)
                    {
                        var InputFileName = Path.GetFileName(file.FileName);
                        var ServerSavePath = Path.Combine(Server.MapPath(folderName), InputFileName);
                        while (System.IO.File.Exists(ServerSavePath))
                        {
                            InputFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + i+Path.GetExtension(file.FileName);
                            ServerSavePath = Path.Combine(Server.MapPath(folderName), InputFileName);
                            i++;
                        }

                        file.SaveAs(ServerSavePath);
                    }
                }
            }
            return RedirectToAction("AddFile", new { id });
        }

        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml, int ID)
        {
            Parcours parcours = db.Parcours.Find(ID);

            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(GridHtml);
                Document pdfDoc = new Document(PageSize.A4, 30f, 10f, 40f, 40f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", parcours.Nom+parcours.Prénom+".pdf");
            }
        }

        public EmptyResult SendMail(int ID)
        {
            var servMiss = db.Missions.Where(s=>s.ID_Parcours == ID).Where(s=>!s.Passage).Select(s => s.Nom_Secteur).ToList();

            List<Service> Serv = new List<Service>();

            foreach(var item in servMiss)
            {
                var original = db.Service.Where(s => s.Nom == item).FirstOrDefault();
                if(original!= null)
                {
                    Serv.Add(original);
                }
            }

            var formateurs = db.Utilisateurs_Services.Where(s => Serv.Contains(s.Service));

            

            return new EmptyResult();
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
