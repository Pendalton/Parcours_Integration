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

namespace Parcours_integration.Controllers
{
    public class OrganiController : BaseController
    {
        private Parcours_IntegrationEntities db = new Parcours_IntegrationEntities();
        // GET: Organi
        public ActionResult Index()
        {
            var list = from m in db.Secteurs
                       orderby m.Nom
                       select m;

            return View(list);
        }

        public ActionResult Carte()
        {
            List<string> TypeRoom = new List<string>();
            var list = from m in db.Plan
                       select m.Type_de_salle;
            TypeRoom.AddRange(list.Distinct());

            var Salles = from m in db.Plan
                         select m;

            ViewBag.Types = TypeRoom;
            return View(Salles);
        }
    }
}