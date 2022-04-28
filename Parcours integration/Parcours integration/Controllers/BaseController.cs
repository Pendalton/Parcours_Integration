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
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();

        public Employes UserSession;
        public bool EstAdmin;
        public bool EstFormateur;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            string login = "";
            if((User != null) && (User.Identity.Name != "")) { login = User.Identity.Name; }
            else { login = Request.LogonUserIdentity.Name; }

            UserSession = GetAccount(login);
            ViewBag.UserSession = UserSession.Nom;
            ViewBag.UserSecteur = UserSession.Secteur;

            EstAdmin = CheckIntervenant(UserSession.Login);
            EstFormateur = CheckFormateur(UserSession.Login);
            ViewBag.EstAdmin = EstAdmin;
        }

        public Employes GetAccount(string login)
        {
            IEnumerable<Employes> u = db.Employes.Where(x => x.Login == login);
            Employes anonyme = new Models.Employes();
            if (u.Count() == 1) { return u.Single(); }
            else if (u.Count() > 1) { anonyme.Nom = "Erreur de Login"; }
            else { anonyme.Nom = "Utilisateur inconnu"; }
            return anonyme;
        }

        public bool CheckIntervenant(string login)
        {
            Employes employe = db.Employes.Where(x => x.Login == login).FirstOrDefault();
            if (employe != null)
            {
                if(employe.Intervenant == true)
                {
                    return true;
                }
                else 
                { 
                    return false; 
                }
            }
            else
            {
                return false;
            }
        }

        public bool CheckFormateur(string login)
        {
            Employes emp = db.Employes.Where(x => x.Login == login).FirstOrDefault();
            if(emp != null)
            {
                return true;
            }
            else { return false; }
        }
    }
}