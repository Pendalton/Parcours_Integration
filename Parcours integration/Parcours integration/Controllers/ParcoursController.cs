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
using Newtonsoft.Json;


namespace Parcours_integration.Controllers
{
    public class ParcoursController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        // GET: Parcours
        public ActionResult Index(int? page, string YearPicker, int? Rythme, bool CDI = true, bool CDD = true, bool Stage = true, bool Mutation = true, bool Terminé = false, bool Intérim = true)
        {
            if (UserService.Select(s=>s.Service.Nom).ToList().Contains(db.Service.Find(16).Nom))
            {
                CDI = CDD = Stage = Mutation = false;
                Intérim = true;
            }

            var parcour = from p in db.Parcours
                          select p;

            List<Parcours> Résultat = new List<Parcours>();
            List<string> Years = new List<string>();

            Years.AddRange(parcour.Select(s => s.Entrée.Value.Year.ToString()).Distinct());

            ViewBag.YearPicker = new SelectList(Years);
            ViewBag.Rythme = new SelectList(db.Equipe, "ID", "Nom");

            if (Rythme != null)
            {
                parcour = parcour.Where(s => s.EquipeID == Rythme);
            }

            if (!String.IsNullOrEmpty(YearPicker))
            {
                parcour = parcour.Where(s => s.Entrée.Value.Year.ToString() == YearPicker);
                Terminé = true;
            }

            if (Terminé)
            {
                parcour = parcour.Where(s => s.Complété == true || s.Complété == false);
            }
            else
            {
                parcour = parcour.Where(s => s.Complété == false);
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


            var SectEmploi = UserService.Select(s => s.Service.Nom).ToList();
            foreach (var par in Résultat.ToList())
            {
                if(par.ID_Resp == UserSession.ID || EstAdmin)
                {
                    continue;
                }
                if (!par.Missions.Any(s => SectEmploi.Contains(s.Nom_Secteur)))
                {
                    Résultat.Remove(par);
                }
            }

            List<Missions> MissTerm = new List<Missions>();
            foreach(var item in Résultat.ToList())
            {
                if(item.ID_Resp == UserSession.ID)
                {
                    var RespService = db.Service.Find(7).Nom;
                    var miss = item.Missions.Where(s=>s.Applicable).Where(s => !s.Passage).Where(s => SectEmploi.Contains(s.Nom_Secteur) || s.Nom_Secteur == RespService).ToList();
                    miss.AddRange(item.Missions.Where(s => s.Applicable).Where(s => s.Passage).Where(s => s.ID_Formateur == UserSession.ID).ToList());
                    miss.OrderBy(s => s.Passage);
                    MissTerm.AddRange(miss);
                }
                else
                {
                    var miss = item.Missions.Where(s => s.Applicable).Where(s => !s.Passage).Where(s => SectEmploi.Contains(s.Nom_Secteur)).ToList();
                    miss.AddRange(item.Missions.Where(s => s.Applicable).Where(s => s.Passage).Where(s => s.ID_Formateur == UserSession.ID).ToList());
                    miss.OrderBy(s => s.Passage);
                    MissTerm.AddRange(miss);
                }
            }

            ViewBag.UserIT = UserService.Any(s => s.ID_Service == 9);

            ViewBag.CDI = CDI;
            ViewBag.CDD = CDD;
            ViewBag.Stage = Stage;
            ViewBag.Mutation = Mutation;
            ViewBag.Intérim = Intérim;
            ViewBag.Term = Terminé;
            ViewBag.MissTerm = MissTerm;

            Résultat = Résultat.OrderByDescending(s=>s.ID).ToList();

            return View(Résultat.ToList().ToPagedList(page?? 1, 10));
        }

        public ActionResult IndexAjax(string YearPicker, int? Rythme, int? page, bool CDI = true, bool CDD = true, bool Stage = true, bool Mutation = true, bool Terminé = false, bool Intérim = true)
        {
            if (UserService.Select(s => s.Service.Nom).ToList().Contains(db.Service.Find(16).Nom))
            {
                CDI = CDD = Stage = Mutation = false;
                Intérim = true;
            }

            var parcour = from p in db.Parcours
                          select p;
            DateTime Picked = DateTime.MinValue;
            DateTime Picked1 = DateTime.MinValue;

            List<Parcours> Résultat = new List<Parcours>();
            List<string> Years = new List<string>();

            Years.AddRange(parcour.Select(s => s.Entrée.Value.Year.ToString()).Distinct());

            ViewBag.YearPicker = new SelectList(Years);



            ViewBag.Rythme = new SelectList(db.Equipe, "ID", "Nom");

            if (Rythme != null)
            {
                parcour = parcour.Where(s => s.EquipeID == Rythme);
            }

            if (!String.IsNullOrEmpty(YearPicker))
            {
                parcour = parcour.Where(s => s.Entrée.Value.Year.ToString() == YearPicker);
                Terminé = true;
            }

            if (Terminé)
            {
                parcour = parcour.Where(s => s.Complété == true || s.Complété == false);
            }
            else
            {
                parcour = parcour.Where(s => s.Complété == false);
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

            var SectEmploi = UserService.Select(s => s.Service.Nom).ToList();
            foreach (var par in Résultat.ToList())
            {
                if (par.ID_Resp == UserSession.ID || EstAdmin)
                {
                    continue;
                }
                if (!par.Missions.Any(s => SectEmploi.Contains(s.Nom_Secteur)))
                {
                    Résultat.Remove(par);
                }
            }

            List<Missions> MissTerm = new List<Missions>();
            foreach (var item in Résultat.ToList())
            {
                if (item.ID_Resp == UserSession.ID)
                {
                    var RespService = db.Service.Find(7).Nom;
                    var miss = item.Missions.Where(s => s.Applicable).Where(s => !s.Passage).Where(s => SectEmploi.Contains(s.Nom_Secteur) || s.Nom_Secteur == RespService).ToList();
                    miss.AddRange(item.Missions.Where(s => s.Applicable).Where(s => s.Passage).Where(s => s.ID_Formateur == UserSession.ID).ToList());
                    miss.OrderBy(s => s.Passage);
                    MissTerm.AddRange(miss);
                }
                else
                {
                    var miss = item.Missions.Where(s => s.Applicable).Where(s => !s.Passage).Where(s => SectEmploi.Contains(s.Nom_Secteur)).ToList();
                    miss.AddRange(item.Missions.Where(s => s.Applicable).Where(s => s.Passage).Where(s => s.ID_Formateur == UserSession.ID).ToList());
                    miss.OrderBy(s => s.Passage);
                    MissTerm.AddRange(miss);
                }
            }

            ViewBag.UserIT = UserService.Any(s => s.ID_Service == 9);

            ViewBag.CDI = CDI;
            ViewBag.CDD = CDD;
            ViewBag.Stage = Stage;
            ViewBag.Mutation = Mutation;
            ViewBag.Intérim = Intérim;
            ViewBag.Term = Terminé;
            ViewBag.MissTerm = MissTerm;

            Résultat = Résultat.OrderByDescending(s => s.ID).ToList();

            return PartialView("TableParc", Résultat.ToList().ToPagedList(page ?? 1, 10));
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

            var RHSign = db.Signatures.Where(s => s.ID_Parcours == parcours.ID).Where(s => s.Role == "Ressources Humaines").FirstOrDefault();
            var RespSign = db.Signatures.Where(s => s.ID_Parcours == parcours.ID).Where(s => s.Role == "Responsable").FirstOrDefault();
            var EmpSign = db.Signatures.Where(s => s.ID_Parcours == parcours.ID).Where(s => s.Role == "Employé" || s.Role == "Admin").FirstOrDefault();

            if(RHSign == null)
            {
                ViewBag.RhSign = null;
            }
            else
            {
                ViewBag.RhSign = new string[] { RHSign.Utilisateurs.Nom, RHSign.Role, RHSign.Date_Signature.ToString().Substring(0,10)};
            }

            if (RespSign == null)
            {
                ViewBag.RespSign = null;
            }
            else
            {
                ViewBag.RespSign = new string[] { RespSign.Utilisateurs.Nom, RespSign.Role, RespSign.Date_Signature.ToString().Substring(0, 10) };
            }

            if (EmpSign == null)
            {
                ViewBag.EmpSign = null;
            }
            else
            {
                ViewBag.EmpSign = new string[] { EmpSign.Utilisateurs.Nom, EmpSign.Role, EmpSign.Date_Signature.ToString().Substring(0, 10) };
            }

            ViewBag.Comments = comments;
            ViewBag.ParcoursID = id.Value;
            ViewBag.Identity = parcours.Nom + " " + parcours.Prénom;
            ViewBag.TypeContrat = parcours.Contrat.Nom + " - " + parcours.Equipe.Nom;
            ViewBag.PosteOccupé = parcours.Poste;
            ViewBag.Entrée = parcours.Entrée.ToString().Substring(0,10);
            ViewBag.Responsable = parcours.Utilisateurs.Nom;

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
            ViewBag.Type_Contrat = new SelectList(db.Contrat.Where(s => s.Actif), "ID", "Nom");
            ViewBag.EquipeID = new SelectList(db.Equipe.Where(s=>s.Actif), "ID", "Nom");
            ViewBag.ID_Resp = new SelectList(db.Utilisateurs.OrderBy(s=>s.Nom), "ID", "Nom");
            ViewBag.ID_Employé = new SelectList(db.Utilisateurs.OrderBy(s => s.Nom), "ID", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nom,Prénom,Entrée,Type_Contrat,Poste,EquipeID,ID_Resp,ID_Employé")] Parcours parcours)
        {

            parcours.Date_entrée = DateTime.Now.Date.ToString().Substring(0, 10);
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
                        Applicable = true,
                    };

                    if (ModelState.IsValid)
                    {
                        db.Missions.Add(miss);
                    }
                }
                db.SaveChanges();
                string name = parcours.ID + parcours.Nom + parcours.Prénom;
                string folderName = @"~/Docs/" + name;

                var mailType = db.Mail.Find(3);
                var NomParcours = parcours.Prénom + " " + parcours.Nom;
                var dateentree = parcours.Entrée.ToString().Substring(0, 10);

                if (!Directory.Exists(Server.MapPath(folderName)))
                {
                    Directory.CreateDirectory(Server.MapPath(folderName));
                }

                if (parcours.ID_Employé != null && parcours.CompteInformatique.UserMail != null)
                {
                    var senderMail = new MailAddress(mailType.SenderMail, mailType.SenderName);

                    var receiverMail = new MailAddress(parcours.CompteInformatique.UserMail);

                    var password = "";

                    var sub = mailType.MailObject;

                    var body = mailType.MailText.Replace("%Nom%", NomParcours).Replace("%Date%", dateentree).Replace("%Lien%", "<a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Formations/Index?Numéro=" + parcours.ID + " \">Parcours de " + NomParcours + " </a>");

                    var smtp = new SmtpClient
                    {
                        Host = "mail-lis.corp.knorr-bremse.com",
                        Port = 25,
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderMail.Address, password),
                    };

                    using (MailMessage mm = new MailMessage(senderMail, receiverMail))
                    {
                        mm.Subject = sub;
                        mm.Body = body;
                        mm.IsBodyHtml = true;
                        smtp.Send(mm);
                    }
                }

                return RedirectToAction("Index");
            }
            ViewBag.Type_Contrat = new SelectList(db.Contrat.Where(s => s.Actif), "ID", "Nom");
            ViewBag.EquipeID = new SelectList(db.Equipe.Where(s => s.Actif), "ID", "Nom");
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
            if(parcours.Type_Contrat != null)
            {
                if (ModelState.IsValid)
                {
                    string folder = @"~/Docs/" + Dossier;
                    string newFold = @"~/Docs/" + parcours.ID + parcours.Nom + parcours.Prénom;
                    var mailType = db.Mail.Find(3);
                    var NomParcours = parcours.Prénom + " " + parcours.Nom;
                    var dateentree = parcours.Entrée.ToString().Substring(0, 10);
                    parcours.CompteInformatique = db.Utilisateurs.Find(parcours.ID_Employé);

                    if (folder != newFold) 
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

                    if (parcours.ID_Employé != null && parcours.CompteInformatique.UserMail != null)
                    {
                        var senderMail = new MailAddress(mailType.SenderMail, mailType.SenderName);

                        var receiverMail = new MailAddress(parcours.CompteInformatique.UserMail);

                        var password = "";

                        var sub = mailType.MailObject;

                        var body = mailType.MailText.Replace("%Nom%", NomParcours).Replace("%Date%", dateentree).Replace("%Lien%", "<a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Formations/Index?Numéro=" + parcours.ID + " \">Parcours de " + NomParcours + " </a>").Replace("\r\n", "</br>");

                        var smtp = new SmtpClient
                        {
                            Host = "mail-lis.corp.knorr-bremse.com",
                            Port = 25,
                            EnableSsl = false,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(senderMail.Address, password),
                        };

                        using (MailMessage mm = new MailMessage(senderMail, receiverMail))
                        {
                            mm.Subject = sub;
                            mm.Body = body;
                            mm.IsBodyHtml = true;
                            smtp.Send(mm);
                        }
                    }

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

            return View(parcours);
        }

        // POST: Parcours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Parcours parcours = db.Parcours.Find(id);
            var miss = db.Missions.Where(s => s.ID_Parcours == id); //vide les misssions
            foreach(var item in miss)
            {
                db.Missions.Remove(item);
            }
            var comm = db.Comment.Where(s => s.ParcoursID == id).FirstOrDefault(); //suppr le comm
            if(comm != null)
            {
                db.Comment.Remove(comm);
            }
            var RespList = db.Parcours.Where(s => s.ID_Resp == parcours.ID_Resp).Where(s=>!s.Complété).ToList(); //dé-responsable le responsable si c'est son dernier parcours
            RespList.Remove(parcours);
            if(RespList.Count() == 0)
            {
                parcours.Utilisateurs.EstResponsable = false;
            }

            var signatures = db.Signatures.Where(s => s.ID_Parcours == id).ToList();
            if(signatures.Count != 0)
            {
                db.Signatures.RemoveRange(signatures);
            }

            string name = parcours.ID + parcours.Nom + parcours.Prénom;
            string folderName = @"~/Docs/" + name;
            if (Directory.Exists(Server.MapPath(folderName)))//supprime le dossier et les docs
            {
                Directory.Delete(Server.MapPath(folderName),true);
            }

            var purge = db.Purge.Where(s => s.ID_Parcours == id).FirstOrDefault();
            if (purge != null)
            {
                db.Purge.Remove(purge);
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
                return File(stream.ToArray(), "application/pdf", parcours.Nom + "_" + parcours.Prénom + ".pdf");
            }
        }

        public void NewParcMail(int ID)
        {
            var servMiss = db.Missions.Where(s => s.ID_Parcours == ID).Where(s => !s.Passage).Where(s => s.Applicable).Select(s => s.Nom_Secteur).Distinct().ToList();
            var NomParcours = db.Parcours.Find(ID).Prénom + " " + db.Parcours.Find(ID).Nom;
            var dateentree = db.Parcours.Find(ID).Entrée.ToString().Substring(0,10);
            List<int> Serv = new List<int>();

            var mailType = db.Mail.Find(1);

            foreach (var item in servMiss)
            {
                var original = db.Service.Where(s => s.Nom == item).FirstOrDefault();
                if (original != null)
                {
                    Serv.Add(original.ID);
                }
            }
            var formateurs = db.Utilisateurs_Services.Where(s => Serv.Contains(s.ID_Service)).Select(s => s.Utilisateurs).Distinct().ToList();

            if (!formateurs.Contains(db.Utilisateurs.Find(db.Parcours.Find(ID).ID_Resp)))
            {
                formateurs.Add(db.Utilisateurs.Find(db.Parcours.Find(ID).ID_Resp));
            }

            foreach (var util in formateurs)
            {
                var ServP = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == util.ID).Select(s => s.Service.Nom).ToList();

                var MissP = db.Missions.Where(s => ServP.Contains(s.Nom_Secteur)).Where(s => s.ID_Parcours == ID).Where(s => !s.Passage).OrderBy(s => s.Nom_Secteur).ToList();

                if (util.UserMail != null)
                {
                    var senderMail = new MailAddress(mailType.SenderMail, mailType.SenderName);

                    var receiverMail = new MailAddress(util.UserMail);

                    var sub = mailType.MailObject.Replace("%Nom%", NomParcours);

                    var body = mailType.MailText.Replace("%Nom%", NomParcours).Replace("%Date%",dateentree).Replace("%Lien%", "<a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Formations/Index?Numéro=" + ID + " \">Parcours de " + NomParcours + " </a>").Replace("\r\n","</br>");
                        
                    var smtp = new SmtpClient
                    {
                        Host = "mail-lis.corp.knorr-bremse.com",
                        Port = 25,
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                    };

                    using (MailMessage mm = new MailMessage(senderMail, receiverMail))
                    {
                        mm.Subject = sub;
                        mm.Body = body;
                        mm.IsBodyHtml = true;
                        smtp.Send(mm);
                    }
                }
            }
        }

        public void SendMail(int ID)
        {
            var servMiss = db.Missions.Where(s=>s.ID_Parcours == ID).Where(s=>!s.Passage).Where(s=>s.Applicable).Select(s => s.Nom_Secteur).Distinct().ToList();
            var NomParcours = db.Parcours.Find(ID).Prénom + " " + db.Parcours.Find(ID).Nom;
            var dateentree = db.Parcours.Find(ID).Entrée.ToString().Substring(0, 10);
            List<int> Serv = new List<int>();

            var mailType = db.Mail.Find(2);

            foreach(var item in servMiss)
            {
                var original = db.Service.Where(s => s.Nom == item).FirstOrDefault();
                if(original!= null)
                {
                    Serv.Add(original.ID);
                }
            }
            var testmail = db.Utilisateurs.Find(199);
            var formateurs = db.Utilisateurs_Services.Where(s => Serv.Contains(s.ID_Service)).Select(s=>s.Utilisateurs).Distinct().ToList();

            if (!formateurs.Contains(db.Utilisateurs.Find(db.Parcours.Find(ID).ID_Resp)))
            {
                formateurs.Add(db.Utilisateurs.Find(db.Parcours.Find(ID).ID_Resp));
            }

            foreach (var util in formateurs)
            {
                var ServP = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == util.ID).Select(s => s.Service.Nom).ToList();

                var MissP = db.Missions.Where(s => ServP.Contains(s.Nom_Secteur)).Where(s => s.ID_Parcours == ID).Where(s=>!s.Passage).OrderBy(s => s.Nom_Secteur).ToList();

                if(util.UserMail != null)
                {
                    var senderMail = new MailAddress(mailType.SenderMail, mailType.SenderName);

                    var receiverMail = new MailAddress(util.UserMail);

                    var password = "";
                    
                    var sub = mailType.MailObject.Replace("%Nom%", NomParcours);

                    var body = mailType.MailText.Replace("%Nom%", NomParcours).Replace("%Date%", dateentree).Replace("%Lien%", "<a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Formations/Index?Numéro=" + ID + " \">Parcours de " + NomParcours + " </a>").Replace("\r\n", "</br>");

                    var smtp = new SmtpClient
                    {
                        Host = "mail-lis.corp.knorr-bremse.com",
                        Port = 25,
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderMail.Address, password),
                    };

                    using(MailMessage mm = new MailMessage(senderMail, receiverMail))
                    {
                        mm.Subject = sub;
                        mm.Body = body;
                        mm.IsBodyHtml = true;
                        smtp.Send(mm);
                    }
                }
            }
        }

        public string Signer(int ID, string RoleBtn)
        {
            var Parc = db.Parcours.Find(ID);
            var Role = "";
            switch (RoleBtn) 
            {
                case "RHbtn":
                    Role = "Ressources Humaines";
                    break;

                case "Respbtn":
                    Role = "Responsable";
                    break;

                case "Empbtn":
                    if (EstAdmin)
                    {
                        Role = "Admin";
                    }
                    else
                    {
                        Role = "Employé";
                    }
                    break;
            }

            if (Parc.Complété)
            {
                var User = UserSession.ID;
                var SignDate = DateTime.Now.Date;

                Signatures Sign = new Signatures
                {
                    ID_Signataire = User,
                    Date_Signature = SignDate,
                    ID_Parcours = Parc.ID,
                    Role = Role,
                };
                db.Signatures.Add(Sign);
                db.SaveChanges();

                if(db.Signatures.Where(s=>s.ID_Parcours == Parc.ID).Count() == 3)
                {
                    if(db.Purge.Where(s=>s.ID_Parcours == Parc.ID).FirstOrDefault() == null)
                    {
                        Purge NewPurge = new Purge
                        {
                            ID_Parcours = Parc.ID,
                            Date_Complétion = DateTime.Now.AddYears(2).Date,
                        };

                        db.Purge.Add(NewPurge);
                        db.SaveChanges();
                    }
                }

                var Data = Tuple.Create(Sign.Date_Signature.ToString().Substring(0, 10), db.Utilisateurs.Find(User).Nom, Role);

                string json = JsonConvert.SerializeObject(Data);

                return json;
            }
            else return null;
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
