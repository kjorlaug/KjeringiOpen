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
                    history.Add(year, loadYear(year));
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
                            instance = loadYear(DateTime.Now.Year);
                        }
                    }
                }

                return instance;
            }
        }

        protected static Race loadYear(int year)
        {
            var jsonFile = System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/" + year.ToString() + ".json");

            if (!System.IO.File.Exists(jsonFile))
                throw new IndexOutOfRangeException("Unsupported year");

            var json = System.IO.File.ReadAllText(jsonFile);

            var race = JsonConvert.DeserializeObject<Race>(json);

            switch (year)
            {
                case 2013:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2014("kjeringi.2013", "2013")).BuildRace(race);
                    break;
                case 2014:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2014("kjeringi.2013", "2014")).BuildRace(race);
                    break;
                case 2015:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2015("kjeringi", new List<int>())).BuildRace(race);
                    break;
            }

            MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Kjeringi.Writer"].ConnectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT card, location, concat(curdate(), \" \", time(time)) as time FROM LocationPasses WHERE year = " + year.ToString() + " ORDER BY time", conn);

            conn.Open();
            var data = cmd.ExecuteReader();

            while (data.Read())
            {
                EmitData d = new EmitData()
                {
                    Id = data.GetInt16("card"),
                    BoxId = data.GetInt16("location"),
                    Time = data.GetDateTime("time"),
                    Force = false
                };
                race.AddPass(d);
            }
            data.Close();
            conn.Close();
            
            return race;
        }
    }
}
