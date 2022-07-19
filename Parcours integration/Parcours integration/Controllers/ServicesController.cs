using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;
using Parcours_integration.Models;
using System.ComponentModel.DataAnnotations;

namespace Parcours_integration.Controllers
{
    public class ServicesController : BaseController
    {
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        // GET: Services
        public ActionResult Index()
        {
            dynamic Ressources = new ExpandoObject();

            var Serv = db.Service.OrderBy(s => s.Nom).ToList();
            var Cont = db.Contrat.OrderBy(s => s.Nom).ToList();
            var Equ = db.Equipe.OrderBy(s => s.Nom).ToList();


            Ressources.Services = Serv.OrderByDescending(s => s.Actif);
            Ressources.Contrats = Cont.OrderByDescending(s => s.Actif);
            Ressources.Equipes = Equ.OrderByDescending(s => s.Actif);

            List<string> ServicesID = new List<string>();
            ServicesID.AddRange(Serv.Select(s => s.ID.ToString()));
            List<string> ContratsID = new List<string>();
            ContratsID.AddRange(Cont.Select(s => s.ID.ToString()));
            List<string> EquipesID = new List<string>();
            EquipesID.AddRange(Equ.Select(s => s.ID.ToString()));

            ViewBag.ServicesID = ServicesID;
            ViewBag.ContratsID = ContratsID;
            ViewBag.EquipesID = EquipesID;

            return View(Ressources);
        }

        //############################################# Partie Services ##########################################

        [HttpPost]
        public ActionResult Insert(string Name, bool Actif)
        {
            dynamic Ressources = new ExpandoObject();

            List<Service> Serv = new List<Service>();
            List<string> ServicesID = new List<string>();

            Service service = new Service
            {
                Actif = Actif,
                Nom = Name,
            };
            if (ModelState.IsValid)
            {
                db.Service.Add(service);
                db.SaveChanges();

                Serv = db.Service.OrderBy(s => s.Nom).ToList();

                Ressources.Services = Serv.OrderByDescending(s => s.Actif);
                ServicesID.AddRange(Serv.Select(s => s.ID.ToString()));
                ViewBag.ServicesID = ServicesID;

                return PartialView("ListServ", Ressources);
            }
            Serv = db.Service.OrderBy(s => s.Nom).ToList();

            Ressources.Services = Serv.OrderByDescending(s => s.Actif);
            ServicesID.AddRange(Serv.Select(s => s.ID.ToString()));
            ViewBag.ServicesID = ServicesID;

            return PartialView("ListServ", Ressources);
        }

        [HttpPost]
        public ActionResult Rename(int ID, string Name)
        {
            var service = db.Service.Find(ID);
            service.Nom = Name;
            db.Entry(service).State = EntityState.Modified;
            db.SaveChanges();

            dynamic Ressources = new ExpandoObject();

            var Serv = db.Service.OrderBy(s => s.Nom).ToList();

            Ressources.Services = Serv.OrderByDescending(s => s.Actif);
            List<string> ServicesID = new List<string>();
            ServicesID.AddRange(Serv.Select(s => s.ID.ToString()));
            ViewBag.ServicesID = ServicesID;

            return PartialView("ListServ", Ressources);
        }

        public ActionResult Desactiver(int? id)
        {
            Service sect = db.Service.Find(id);
            sect.Actif = !sect.Actif;
            db.SaveChanges();

            dynamic Ressources = new ExpandoObject();

            var Serv = db.Service.OrderBy(s => s.Nom).ToList();

            Ressources.Services = Serv.OrderByDescending(s => s.Actif);
            List<string> ServicesID = new List<string>();
            ServicesID.AddRange(Serv.Select(s => s.ID.ToString()));
            ViewBag.ServicesID = ServicesID;

            return PartialView("ListServ", Ressources);
        }

        //############################################# Partie Contrats ##########################################

        [HttpPost]
        public ActionResult InsertCont(string Name, bool Actif)
        {
            dynamic Ressources = new ExpandoObject();

            List<Contrat> Cont = new List<Contrat>();
            List<string> ContratsID = new List<string>();


            Contrat contrat = new Contrat
            {
                Actif = Actif,
                Nom = Name,
            };
            if (ModelState.IsValid)
            {
                db.Contrat.Add(contrat);
                db.SaveChanges();

                Cont = db.Contrat.OrderBy(s => s.Nom).ToList();

                Ressources.Contrats = Cont.OrderByDescending(s => s.Actif);
                ContratsID.AddRange(Cont.Select(s => s.ID.ToString()));
                ViewBag.ContratsID = ContratsID;

                return PartialView("ListCont", Ressources);
            }

            Cont = db.Contrat.OrderBy(s => s.Nom).ToList();

            Ressources.Contrats = Cont.OrderByDescending(s => s.Actif);
            ContratsID.AddRange(Cont.Select(s => s.ID.ToString()));
            ViewBag.ContratsID = ContratsID;

            return PartialView("ListCont", Ressources);
        }

        [HttpPost]
        public ActionResult RenameCont(int ID, string Name)
        {
            var contrat = db.Contrat.Find(ID);
            contrat.Nom = Name;
            db.Entry(contrat).State = EntityState.Modified;
            db.SaveChanges();

            dynamic Ressources = new ExpandoObject();

            var Cont = db.Contrat.OrderBy(s => s.Nom).ToList();

            Ressources.Contrats = Cont.OrderByDescending(s => s.Actif);
            List<string> ContratsID = new List<string>();
            ContratsID.AddRange(Cont.Select(s => s.ID.ToString()));
            ViewBag.ContratsID = ContratsID;

            return PartialView("ListCont", Ressources);
        }

        public ActionResult DesactiverCont(int? id)
        {
            Contrat contrat = db.Contrat.Find(id);
            contrat.Actif = !contrat.Actif;
            db.Entry(contrat).State = EntityState.Modified;
            db.SaveChanges();

            dynamic Ressources = new ExpandoObject();

            var Cont = db.Contrat.OrderBy(s => s.Nom).ToList();
            
            Ressources.Contrats = Cont.OrderByDescending(s => s.Actif);
            List<string> ContratsID = new List<string>();
            ContratsID.AddRange(Cont.Select(s => s.ID.ToString()));
            ViewBag.ContratsID = ContratsID;

            return PartialView("ListCont", Ressources);
        }

        //############################################# Partie Equipes ###########################################

        [HttpPost]
        public ActionResult InsertEqu(string Name, bool Actif)
        {
            dynamic Ressources = new ExpandoObject();

            List<Equipe> Equ = new List<Equipe>();
            List<string> EquipesID = new List<string>();

            Equipe equipe = new Equipe
            {
                Actif = Actif,
                Nom = Name,
            };
            if (ModelState.IsValid)
            {
                db.Equipe.Add(equipe);
                db.SaveChanges();

                Equ = db.Equipe.OrderBy(s => s.Nom).ToList();

                Ressources.Equipes = Equ.OrderByDescending(s => s.Actif);

                EquipesID.AddRange(Equ.Select(s => s.ID.ToString()));

                ViewBag.EquipesID = EquipesID;

                return PartialView("ListEqu", Ressources);
            }

            Equ = db.Equipe.OrderBy(s => s.Nom).ToList();

            Ressources.Equipes = Equ.OrderByDescending(s => s.Actif);
            EquipesID.AddRange(Equ.Select(s => s.ID.ToString()));

            ViewBag.EquipesID = EquipesID;

            return PartialView("ListEqu", Ressources);
        }

        [HttpPost]
        public ActionResult RenameEqu(int ID, string Name)
        {
            dynamic Ressources = new ExpandoObject();

            var equipe = db.Equipe.Find(ID);
            equipe.Nom = Name;
            db.Entry(equipe).State = EntityState.Modified;
            db.SaveChanges();

            var Equ = db.Equipe.OrderBy(s => s.Nom).ToList();

            Ressources.Equipes = Equ.OrderByDescending(s => s.Actif);
            List<string> EquipesID = new List<string>();
            EquipesID.AddRange(Equ.Select(s => s.ID.ToString()));

            ViewBag.EquipesID = EquipesID;

            return PartialView("ListEqu", Ressources);
        }

        public ActionResult DesactiverEqu(int? id)
        {
            Equipe equipe = db.Equipe.Find(id);
            equipe.Actif = !equipe.Actif;
            db.Entry(equipe).State = EntityState.Modified;
            db.SaveChanges();

            dynamic Ressources = new ExpandoObject();

            var Equ = db.Equipe.OrderBy(s => s.Nom).ToList();

            Ressources.Equipes = Equ.OrderByDescending(s => s.Actif);
            List<string> EquipesID = new List<string>();
            EquipesID.AddRange(Equ.Select(s => s.ID.ToString()));

            ViewBag.EquipesID = EquipesID;

            return PartialView("ListEqu", Ressources);
        }

        //########################################################################################################

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
