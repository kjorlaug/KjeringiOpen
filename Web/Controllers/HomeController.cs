using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EmitReader()
        {
            return View();
        }

        public ActionResult Station(String name)
        {
            ViewBag.Name = name;

            return View();
        }

        public ActionResult Reset()
        {
            KjeringiData.Konkurranse.Reset();
            return RedirectToAction("Index");
        }

    }
}