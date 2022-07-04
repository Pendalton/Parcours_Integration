﻿using System;
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
        public bool EstResponsable;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            string login;
            if ((User != null) && (User.Identity.Name != "")) { login = User.Identity.Name; }
            else { login = Request.LogonUserIdentity.Name; }

            UserSession = GetAccount(login);
            ViewBag.UserSession = UserSession.Nom;
            UserService = db.Utilisateurs_Services.Where(s => s.ID_Utilisateur == UserSession.ID).ToList();

            ViewBag.UserSecteur = UserService;

            ViewBag.IsAdmin = EstAdmin = UserSession.EstAdmin;
            ViewBag.EstFormateur = EstFormateur = UserSession.EstFormateur;
            ViewBag.EstRH = EstRH = UserService.Contains(UserService.Where(s => s.ID_Service == 5).FirstOrDefault());
            ViewBag.EstResponsable = EstResponsable = UserSession.EstResponsable;

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
    }
}