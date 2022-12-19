using Antlr.Runtime;
using Parcours_integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Parcours_integration.Controllers
{
    public class HomeController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();
        public ActionResult Index()
        {
            if (EstAdmin || EstFormateur || EstResponsable)
            {
                return RedirectToAction("Index", "Formations");
            }
            else
            {
                var TestParc = db.Parcours.Where(s=>s.CompteInformatique.ID == UserSession.ID || s.Nom + " " + s.Prénom == UserSession.Nom).FirstOrDefault();

                var TempParcUser = new TempParc();

                List<string> services = new List<string>();
                List<Missions> ListMiss = new List<Missions>();
                switch (TestParc != null)
                {
                    case true:
                        TempParcUser = new TempParc
                        {
                            Nom = TestParc.Nom,
                            Prénom = TestParc.Prénom,
                            Responsable = TestParc.Utilisateurs.Nom,
                            Contrat = TestParc.Contrat.Nom,
                            Poste = TestParc.Poste,
                            Equipe = TestParc.Equipe.Nom,
                            Date = TestParc.Entrée.ToString().Substring(0,10),
                        };

                        foreach(var missions in TestParc.Missions)
                        {
                            ListMiss.Add(missions);
                            if (!missions.Passage)
                            {
                                services.Add(missions.Nom_Secteur);
                            }
                        }
                        services = services.Distinct().ToList();
                        ListMiss = ListMiss.OrderBy(s => s.Nom_Secteur).OrderBy(s=>s.Passage).ToList();
                        ViewBag.Services = services;
                        ViewBag.ListMiss = ListMiss;
                        break;

                    case false:
                        TempParcUser = new TempParc
                        {
                            Nom = "Utilisateur",
                            Prénom = "Inconnu",
                            Responsable = "Aucun",
                            Contrat = "NaN",
                            Poste = "NaN",
                            Equipe = "NaN",
                            Date = "--/--/----"
                        };

                        string txt = "Aucun parcours reconnu.";
                        services.Add(txt);
                        ViewBag.Services = services;
                        ViewBag.ListMiss = ListMiss;

                        break;
                }
                return View(TempParcUser);
            }
        }
        public class TempParc
        {
            public string Nom { get; set; }
            public string Prénom { get; set; }
            public string Responsable { get; set; }
            public string Contrat { get; set; }
            public string Poste { get; set; }
            public string Equipe { get; set; }
            public string Date { get; set; }
        }
    }
}