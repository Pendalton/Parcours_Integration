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

            ViewBag.IsAdmin = EstAdmin = UserSession.EstAdmin; //Si admin
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

                foreach(var parc in ParcP.ToList())
                {
                    var Name = db.Missions.Where(s => !s.Passage).Where(s => s.Applicable).Where(s => s.ID_Parcours == parc.ID).Select(s => s.Nom_Secteur).Distinct().ToList();

                    if(!Name.Any(x => ServP.Contains(x)))
                    {
                        ParcP.Remove(parc);
                    }
                }

                if (util.UserMail != null)
                {
                    var senderMail = new MailAddress("ServiceRHLisieux@knorr-bremse.com", "Service RH");

                    var receiverMail = new MailAddress(util.UserMail);

                    var password = "";

                    var sub = "Relance des parcours d'intégration";

                    var body = $"{util.Nom}, il vous reste {ParcP.Count()} formation(s) à effectuer sur des parcours d'intégration. Cliquez le lien suivant pour avoir accès aux formations en question.<br/><br/><a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Formations/Index\">Liste des parcours à faire</a>" +

                        $"<br/><br/>" +
                        $"<table>" +
                        $"<th><tr style=\"background-color:#efefef;border:1px solid black\"><td>Nom du parcours</td><td>Formations à faire</td><td>Détails du parcours</td></tr></th><tbody>";

                    var Content = "";
                    foreach (var item in ParcP)
                    {
                        Content += "<tr style=\"border:1px solid black \">" +
                                       "<td><span style=\"font-style:bold\">" + item.Nom + "</span> " + item.Prénom + "</td>" +
                                       "<td>" + item.Missions.Where(s=>!s.Passage).Where(s => ServP.Contains(s.Nom_Secteur)).Count() + "</td>" +
                                       "<td> <a href=\"http://liss4022.corp.knorr-bremse.com/ParcoursIntegration/Parcours/Details/" + item.ID + "\">Parcours</a></td>" +
                                   "</tr>";
                    }

                    var end = "</tbody>" + "</table>";

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
                        mm.Body = body + Content + end;
                        mm.IsBodyHtml = true;
                        smtp.Send(mm);
                        ViewBag.Message = "Mail envoyé";
                    }
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