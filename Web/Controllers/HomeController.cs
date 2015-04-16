using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using KjeringiData;
using Newtonsoft.Json;

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

            //Plassering plassering = Konkurranse.GetInstance.Plasseringar.Find(x => x.Namn.Equals(Server.HtmlDecode(name)));
            //if (plassering != null)
            //    ViewBag.Index = plassering.Sekvens - 1;
            //else
                ViewBag.Index = 0;

            return View();
        }

        public ActionResult Reset()
        {
            //KjeringiData.Konkurranse.Reset();
            return RedirectToAction("Index");
        }

        public ActionResult Store()
        {
            var json = JsonConvert.SerializeObject(TheRace.Instance, Formatting.Indented);
            System.IO.File.WriteAllText(Server.MapPath(@"~/App_Data/" + TheRace.Instance.Name + ".json"), json);

            //KjeringiData.Konkurranse.Reset();
            return RedirectToAction("Index");
        }

    }
}