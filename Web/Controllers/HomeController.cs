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

        public ActionResult Participants()
        {            
            return View();
        }

        public ActionResult Results()
        {
            return View();
        }

        public ActionResult Station(String name)
        {
            ViewBag.StationId = name;
            ViewBag.StationName = TheRace.Instance.TimeStations.Find(ts => ts.Id.ToString().Equals(name)).Name;

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