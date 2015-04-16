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

        public static Race Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null) {
                            var json = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/2014.json"));
                            instance = JsonConvert.DeserializeObject<Race>(json);
                            (new EmitReaderLib.Builders.MySqlRaceBuilder2014("kjeringi", "2014")).BuildRace(instance);

                        }
                    }
                }

                return instance;
            }
        }
    }
}
