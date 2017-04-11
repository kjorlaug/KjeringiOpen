using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EmitReaderLib;
using EmitReaderLib.Builders;
using EmitReaderLib.Model;
using EmitReaderLib.Writers;
using EmitReaderLib.Workers;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;

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

            lock (syncRoot)
            {
                if (!history.ContainsKey(year))
                    history.Add(year, Race.LoadYear(year, System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/" + year.ToString() + ".json")));
            }
            return history[year];
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
                            //instance = Race.LoadYear(DateTime.Now.Year, System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/" + DateTime.Now.Year.ToString() + ".json"));
                            instance = Race.LoadYear(2016, System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/2016.json"));
                        }
                    }
                }

                return instance;
            }
        }
    }
}
