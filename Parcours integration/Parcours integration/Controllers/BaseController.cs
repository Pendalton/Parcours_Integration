using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Text.RegularExpressions;
using Parcours_integration.Models;
using System.util;
using System.Web.UI.WebControls.WebParts;

namespace Parcours_integration.Controllers
{
    public class BaseController : Controller
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        public Utilisateurs UserSession;
        public List<Utilisateurs_Services> UserService;
        public bool EstAdmin;
        public bool EstFormateur;
        public bool EstRH;
        public bool EstManPower;
        public bool EstResponsable;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            string login;
            if ((User != null) && (User.Identity.Name != "")) { login = User.Identity.Name; }
            else { login = Request.LogonUserIdentity.Name; }

            UserSession = GetAccount(login); //UserSession est toutes les infos de l'utilisateur
            ViewBag.UserSession = UserSession.Nom;

            UserService = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == UserSession.ID).ToList(); //liste des services de l'utilisateur
            ViewBag.UserID = UserSession.ID;
            ViewBag.UserSecteur = UserService;

            ViewBag.EstAdmin = EstAdmin = UserSession.EstAdmin; //Si admin
            ViewBag.EstFormateur = EstFormateur = UserSession.EstFormateur; //Si est un Formateur
            ViewBag.EstRH = EstRH = UserService.Any(s => s.ID_Service == 5); //Si RH
            ViewBag.EstManPower = EstManPower = UserService.Any(s => s.ID_Service == 16);
            ViewBag.EstResponsable = EstResponsable = UserSession.EstResponsable; //Si responsable d'un employé
        }

        public Utilisateurs GetAccount(string login)
        {
            IEnumerable<Utilisateurs> u = db.Utilisateurs.Where(x => x.Login == login);
            Utilisateurs anonyme = new Models.Utilisateurs();
            if (u.Count() == 1) { return u.Single(); }
            else if (u.Count() > 1) { anonyme.Nom = "Erreur de Login"; }
            else { anonyme.Nom = "Utilisateur inconnu"; }
            return anonyme;
        }

        public void SendMails()
        {
            var servMiss = db.Missions.Where(s => !s.Passage).Select(s => s.Nom_Secteur).Distinct().ToList();
            List<int> Serv = new List<int>();

            var mailType = db.Mail.Find(2);

            foreach (var item in servMiss)
            {
                var original = db.Service.Where(s => s.Nom == item).FirstOrDefault();
                if (original != null)
                {
                    Serv.Add(original.ID);
                }
            }
            var formateurs = db.Utilisateurs_Services.Where(s => Serv.Contains(s.ID_Service)).Select(s => s.Utilisateurs).Distinct().ToList();
            foreach (var util in formateurs)
            {
                var ServP = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == util.ID).Select(s => s.Service.Nom).ToList();

                var ParcP = db.Parcours.Where(s => !s.Complété).OrderBy(s => s.Nom).ToList();
                int missNbr = 0;
                foreach(var parc in ParcP.ToList())
                {
                    var Name = db.Missions.Where(s => !s.Passage).Where(s => s.Applicable).Where(s => s.ID_Parcours == parc.ID).Select(s => s.Nom_Secteur).Distinct().ToList();

                    if(!Name.Any(x => ServP.Contains(x)))
                    {
                        ParcP.Remove(parc);
                    }
                    else
                    {
                        missNbr += db.Missions.Where(s => !s.Passage).Where(s => s.Applicable).Where(s => s.ID_Parcours == parc.ID).Where(s=>ServP.Contains(s.Nom_Secteur)).Count();
                    }
                }

                if (util.UserMail != null)
                {
                    var senderMail = new MailAddress("service-rhlisieux@knorr-bremse.com", "Service RH");

                    var receiverMail = new MailAddress("felix.delesalle@knorr-bremse.com");

                    var sub = "Relance des parcours d'intégration";

                    var body = $"{util.Nom}, il vous reste {missNbr} formation(s) à effectuer sur des parcours d'intégration. Cliquez les liens suivants pour avoir accès aux formations en question.<br/><br/><a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Formations/Index\">Liste des parcours à faire</a>" +

                        $"<br/><br/>" +
                        $"<ul> ";

                    var Content = "";
                    foreach (var item in ParcP)
                    {
                        Content += $"<li>" +
                        $"<a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Parcours/Details/" + item.ID + "\">" + item.Nom + " " + item.Prénom + "</a>, " + item.Poste + " embauché(e) le " + item.Entrée.ToString().Substring(0,10) + 
                        $"</li>";
                    }

                    var end = "</ul>" +
                        "<br/>" +
                        "Si vous avez des remarques ou questions, n’hésitez pas à contacter le service RH.<br/>Merci.";

                    var smtp = new SmtpClient
                    {
                        Host = "mail-lis.corp.knorr-bremse.com",
                        Port = 25,
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false
                    };

                    using (MailMessage mm = new MailMessage(senderMail, receiverMail))
                    {
                        mm.Subject = sub;
                        mm.Body = body + Content + end;
                        mm.IsBodyHtml = true;
                        smtp.Send(mm);
                    }
                }
            }
        }

        public void CompletionMail(int ID)
        {
            var parcours = db.Parcours.Find(ID);
            List<Utilisateurs> Mails = new List<Utilisateurs>();
            if(parcours.Type_Contrat == 7)
            {
                var temp = db.Utilisateurs_Services.Where(s => s.ID_Service == 16).Select(s=>s.Utilisateurs).ToList();
                Mails.AddRange(temp);
            }
            else
            {
                var temp = db.Utilisateurs_Services.Where(s => s.ID_Service == 5).Select(s => s.Utilisateurs).ToList(); 
                Mails.AddRange(temp);
            }
            Mails.Add(parcours.Utilisateurs);
            if(parcours.CompteInformatique != null)
            {
                Mails.Add(parcours.CompteInformatique);
            }

            foreach(var mail in Mails)
            {
                var senderMail = new MailAddress("service-rhlisieux@knorr-bremse.com", "Service RH");

                var receiverMail = new MailAddress(mail.UserMail);

                var sub = "Fin du parcours, Signatures requises";

                var body = $"Le parcours de {parcours.Nom + " " + parcours.Prénom} vient d'être complété. Merci de signer le parcours en cliquant sur le lien ci-dessous :" +
                    "<br/><br/>" + 
                    $"<a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Parcours/Details/" + parcours.ID + "\">" + parcours.Nom + " " + parcours.Prénom + "</a>, " + parcours.Poste + ", embauché(e) le " + parcours.Entrée.ToString().Substring(0, 10) +
                    ".<br/><br/>" +
                    "Si vous avez des remarques ou questions, n’hésitez pas à contacter le service RH.<br/>Merci.";

                var smtp = new SmtpClient
                {
                    Host = "mail-lis.corp.knorr-bremse.com",
                    Port = 25,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                using (MailMessage mm = new MailMessage(senderMail, receiverMail))
                {
                    mm.Subject = sub;
                    mm.Body = body;
                    mm.IsBodyHtml = true;
                    smtp.Send(mm);
                    ViewBag.Message = "Mail envoyé";
                }
            }
        }

        public void Purge()
        {
            foreach(var Parcours in db.Purge.ToList())
            {
                if(DateTime.Now.Date >= Parcours.Date_Complétion)
                {
                    new ParcoursController().Delete(Parcours.ID_Parcours);
                    db.Purge.Remove(Parcours);
                }
            }
        }
    }
}