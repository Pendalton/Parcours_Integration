using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Parcours_integration.Controllers
{
    public class OrganiController : BaseController
    {
        // GET: Organi
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Carte()
        {
            return View();
        }
    }
}