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
        private ParcoursIntegrationEntities db = new ParcoursIntegrationEntities();

        public ActionResult Carte()
        {
            return View();
        }
    }
}