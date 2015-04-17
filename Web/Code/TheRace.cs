using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EmitReaderLib;
using EmitReaderLib.Builders;
using EmitReaderLib.Model;
using EmitReaderLib.Writers;
using Newtonsoft.Json;

namespace KjeringiData
{
    public sealed class TheRace
    {
        private static volatile Race instance;
        private static object syncRoot = new Object();

        private TheRace() { }

        private static Dictionary<int, Race> history = new Dictionary<int, Race>();

        public static Race Historical(int year)
        {
            if (year == int.Parse(Instance.Name))
                return Instance;

            if (history.ContainsKey(year))
                return history[year];

            var json = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/" + year.ToString() + ".json"));
            var r  = JsonConvert.DeserializeObject<Race>(json);

            r.Initialize();

            history.Add(year, r);
            return r;
        }

        public static Race Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null) {
                            //var json = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/2013.json"));
                            //instance = JsonConvert.DeserializeObject<Race>(json);
                            //(new EmitReaderLib.Builders.MySqlRaceBuilder2014("kjeringi.2013", "2013")).BuildRace(instance);

                            //var json = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/2014.json"));
                            //instance = JsonConvert.DeserializeObject<Race>(json);
                            //(new EmitReaderLib.Builders.MySqlRaceBuilder2014("kjeringi.2013", "2014")).BuildRace(instance);

                            var json = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/2015.json"));
                            instance = JsonConvert.DeserializeObject<Race>(json);
                            instance.Testers = new List<int>() { 1097, 1098, 1099 };
                            (new EmitReaderLib.Builders.MySqlRaceBuilder2015("kjeringi", instance.Testers)).BuildRace(instance);
                        }
                    }
                }

                return instance;
            }
        }
    }
}
