using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using EmitReaderLib.Writers;
using EmitReaderLib.Utils;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace EmitReaderLib
{
    public class Race
    {
        private object syncRoot = new Object();

        public Race()
        {
            Classes = new List<ParticipantClass>();
            Participants = new List<Participant>();
            TimeStations = new List<TimeStation>();
            Passes = new List<EmitData>();
            Testers = new List<int>();

            Legs = new Leg[]
            {
                new Leg() {Name = "Telemark", Icon = "icon-Downhill", TimestationId = 90 },
                new Leg() {Name = "Ski", Icon = "icon-Skiing", TimestationId = 91 },
                new Leg() {Name = "Springing", Icon = "icon-Running",TimestationId = 92 },
                new Leg() {Name = "Sykling", Icon = "icon-cycling", TimestationId = 248 }
            };
        }

        public String Name { get; set; }

        public Leg[] Legs { get; set; }

        public List<ParticipantClass> Classes { get; protected set; }
        public List<Participant> Participants { get; protected set; }
        public List<TimeStation> TimeStations { get; protected set; }

        public List<int> Testers { get; set; }

        protected List<EmitData> Passes { get; set; }
        public Boolean InTestMode { get; set; }

        public void AddParticipant(Participant p)
        {
            p.Race = this;
            Participants.Add(p);
        }

        public void AddOrUpdateParticipant(Participant p)
        {
            // Already there?
            Participant par = Participants.Find(pp => pp.Id.Equals(p.Id));
            p.Race = this;

            if (par == null)
                Participants.Add(p);
            else {
                JObject existing = JObject.Parse(JsonConvert.SerializeObject(par));
                JObject updated = JObject.Parse(JsonConvert.SerializeObject(p));
                if (!JToken.DeepEquals(existing, updated))
                {
                    Participants.Remove(par);
                    Participants.Add(p);
                }
            }
        }

        public Participant AddPass(EmitData emitdata) {

            // Not a valid participant - ignore...
            if (!Participants.Exists(p => p.EmitID == emitdata.Id))
                return null;

            // Find participant
            var participant = Participants.Where(p => p.EmitID == emitdata.Id).First();
            
            // And reporting timestation
            var timestation = TimeStations.Find(x => x.Id.Equals(emitdata.BoxId));

            lock (syncRoot)
            {
                // Update participant
                if (!participant.Passes.ContainsKey(timestation.Id))
                    participant.Passes.Add(timestation.Id, emitdata);
                else if (participant.Passes[timestation.Id].Estimated || emitdata.Force)
                    participant.Passes[timestation.Id] = emitdata;

                DateTime startTime = participant.Passes.First().Value.Time;
                TimeSpan totalTime = (emitdata.Time - startTime);
                participant.CurrentTime = totalTime;

                if (timestation.Official)
                {
                    List<Result> res = new List<Result>();

                    foreach (ParticipantClass c in participant.Classes)
                    {
                        res.AddRange(
                            participant.Passes
                                .Where(p => this.TimeStations.Find(ts => ts.Id.Equals(p.Key)).Official)
                                .OrderBy(p => this.TimeStations.Find(ts => ts.Id.Equals(p.Key)).Sequence)
                                .SelectWithPrevious((prev, cur) =>
                                    new Result()
                                    {
                                        Class = c.Name,
                                        EmitId = participant.EmitID,
                                        Sequence = this.TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Sequence,
                                        Location = cur.Key,
                                        IsSuper = participant.IsSuper,
                                        Startnumber = participant.Startnumber,
                                        Leg = this.TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Name,
                                        ClassId = c.Id,
                                        Name = participant.IsSuper ? participant.Name : participant.TeamMembers[this.TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Leg - 1],
                                        Team = participant.IsSuper ? "" : participant.Name,
                                        Time = (cur.Value.Time - prev.Value.Time).Hours > 0 ? (cur.Value.Time - prev.Value.Time).ToString(@"hh\:mm\:ss") : (cur.Value.Time - prev.Value.Time).ToString(@"mm\:ss"),
                                        Estimated = (cur.Value.Estimated || prev.Value.Estimated),
                                        Ticks = (cur.Value.Time - prev.Value.Time).Ticks,
                                        TicksSoFar = (cur.Value.Time - startTime).Ticks,
                                        Total = participant.TotalTime,
                                        Position = this.Participants.Where(p => p.Classes.Exists(pc => pc.Id.Equals(c.Id)) && p.Passes.ContainsKey(cur.Key))
                                            .OrderBy(p => p.Passes[cur.Key].Time)
                                            .ToList<Participant>()
                                            .IndexOf(participant) + 1

                                    }
                                        ).ToList<Result>());
                    }
                    participant.TotalTime = totalTime.ToString(@"hh\:mm\:ss");

                    participant._splits = res.OrderBy(r => r.Sequence).ThenBy(r => r.Position).ToList<Result>();

                    participant.LegSplits = new List<string>();
                    participant.LegEstimated = new List<Boolean>();

                    foreach(TimeStation ts in this.TimeStations.Where(ts => ts.Official && !ts.Start).OrderBy(ts => ts.Sequence)) {
                        if (participant.Leg(participant.Classes.First().Id, ts.Id) != null)
                        {
                            participant.LegSplits.Add(participant.Leg(participant.Classes.First().Id, ts.Id).Time);
                            participant.LegEstimated.Add(participant.Leg(participant.Classes.First().Id, ts.Id).Estimated);
                        }
                        else
                        {
                            participant.LegSplits.Add("");
                            participant.LegEstimated.Add(false);
                        }
                    }
                }

                if (timestation.Finish)
                {
                    participant.Finished = true;
                    participant.RealArrival = emitdata.Time;
                }

                return participant;
            }
        }

        public ICollection<Participant> GetResults(String participantClassId)
        {
            if (participantClassId.StartsWith("NM"))
            {
                return Participants.Where(p => p.Classes.Exists(pc => pc.Id.Equals(participantClassId)))
                    .OrderBy(p => p._splits.Count() > 0 ? p._splits[0].Ticks : -1)
                    .ThenByDescending(p => p._splits.Count())
                    .ThenBy(p => p.Startnumber).ToList<Participant>();
            }
            else
            {
                return Participants.Where(p => p.Classes.Exists(pc => pc.Id.Equals(participantClassId)))
                    .OrderBy(p => !p.Finished)
                    .ThenBy(p => p.RealArrival)
                    .ThenByDescending(p => p._splits.Count())
                    .ThenBy(p => p.Startnumber).ToList<Participant>();
            }
        }

        public ICollection<Participant> GetBySplits(int stationId, int ageClass, int gender)
        {
            int split = 0;
            if (stationId == 91)
                split = 1;
            else if (stationId == 92)
                split = 2;
            else if (stationId == 248)
                split = 3;

            List<Participant> res = Participants.Where(p => p.Finished && !p.Classes.Exists(pc => pc.Id.Equals("TEST")))
                .OrderBy(p => p._splits[split * p.Classes.Count].Ticks).ToList<Participant>();

            if (ageClass == 1) // Junior 10-16
                res = res.Where(p => p.Ages[p.IsSuper ? 0 : split] <= 16).ToList<Participant>();
            else if (ageClass == 2) // Senior 17-39
                res = res.Where(p => p.Ages[p.IsSuper ? 0 : split] >= 17 && p.Ages[p.IsSuper ? 0 : split] <= 39).ToList<Participant>();
            else if (ageClass == 3) // Veteran 40-49
                res = res.Where(p => p.Ages[p.IsSuper ? 0 : split] >= 40 && p.Ages[p.IsSuper ? 0 : split] <= 49).ToList<Participant>();
            else if (ageClass == 4) // Superveteran 50
                res = res.Where(p => p.Ages[p.IsSuper ? 0 : split] >= 50).ToList<Participant>();

            if (gender == 1) //men
                res = res.Where(p => p.Gender[p.IsSuper ? 0 : split] == 1).ToList<Participant>();
            else if(gender == 2) //women
                res = res.Where(p => p.Gender[p.IsSuper ? 0 : split] == 2).ToList<Participant>();

            return res;
        }

        public void Initialize()
        {
            //foreach (Participant p in Participants)
            //    IndexParticipant(p);
        }

        private static string NormalizeAzureInAppConnString(string raw)
        {
            string conn = string.Empty;
            try
            {
                var dict =
                     raw.Split(';')
                         .Where(kvp => kvp.Contains('='))
                         .Select(kvp => kvp.Split(new char[] { '=' }, 2))
                         .ToDictionary(kvp => kvp[0].Trim(), kvp => kvp[1].Trim(), StringComparer.InvariantCultureIgnoreCase);
                var ds = dict["Data Source"];
                var dsa = ds.Split(':');
                conn = $"Server={dsa[0]};Port={dsa[1]};Uid={dict["User Id"]};Pwd={dict["Password"]};SslMode=none;";
            }
            catch
            {
                throw new Exception("unexpected connection string: datasource is empty or null");
            }
            return conn;
        }

        public static Race LoadYear(int year, String jsonFile)
        {
            if (!System.IO.File.Exists(jsonFile))
                throw new IndexOutOfRangeException("Unsupported year", new Exception(year.ToString()));

            var json = System.IO.File.ReadAllText(jsonFile);

            var race = JsonConvert.DeserializeObject<Race>(json);

            // Create correct connection string
            //String t = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");
            String t = NormalizeAzureInAppConnString(System.Configuration.ConfigurationManager.ConnectionStrings["localdb"].ConnectionString);

            if (String.IsNullOrEmpty(t))
                t = System.Configuration.ConfigurationManager.ConnectionStrings["Kjeringi"].ConnectionString;

            race.InTestMode = true;
            switch (year)
            {                
                case 2013:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2014(t + "Database=kop2015;", "2013")).BuildRace(race);
                    break;
                case 2014:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2014(t + "Database=kop2015;", "2014")).BuildRace(race);
                    break;
                case 2015:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2015(t + "Database=kop2015;", new List<int>(), "2015", new DateTime(2015, 4, 18))).BuildRace(race);
                    break;
                case 2016:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2015(t + "Database=kop2016;", Enumerable.Range(5001, 29).ToList<int>(), "2016", new DateTime(2016, 4, 16))).BuildRace(race);
                    break;
                case 2017:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2015(t + "Database=kop2017;", Enumerable.Range(5001, 29).ToList<int>(), "2017", new DateTime(2017, 4, 22))).BuildRace(race);
                    break;
                case 2018:
                    (new EmitReaderLib.Builders.JsonRaceBuilder(jsonFile.Replace(".json", "_%source%.json"), Enumerable.Range(4480, 20).ToList<int>(), "2018", new DateTime(2018, 4, 14))).BuildRace(race);
                    break;
                case 2019:
                    (new EmitReaderLib.Builders.JsonRaceBuilder(jsonFile.Replace(".json", "_%source%.json"), Enumerable.Range(3681, 20).ToList<int>(), "2019", new DateTime(2019, 4, 27))).BuildRace(race);
                    race.Legs[1].Name = "Terrengløp";
                    race.Legs[1].Icon = "icon-trail";
                    break;
                    //case 2019:
                    //    (new EmitReaderLib.Builders.RestRaceBuilder("https://www.skriki.no/kop19/ipa/%source%/fetchAll?token=98dbf8596407ab5f896ff5b8e286631c7dc0ed200a610565ae860ad13f75eefa", Enumerable.Range(3681, 20).ToList<int>(), "2019", new DateTime(2019, 4, 27))).BuildRace(race);
                    //    race.Legs[1].Name = "Terrengløp";
                    //    race.Legs[1].Icon = "icon-trail";

                    //    break;
            }

            var resultsFile = jsonFile.Replace(".json", "_results.json");

            if (System.IO.File.Exists(resultsFile))
            {
                var results = JArray.Parse(System.IO.File.ReadAllText(resultsFile));

                foreach (dynamic r in results.Children<JObject>())
                {
                    EmitData d = new EmitData()
                    {
                        Id = (int)r.Card,
                        BoxId = (int)r.Location,
                        Time = (DateTime)r.Time
                    };
                    race.AddPass(d);
                }

            }


            //MySqlConnection conn = new MySqlConnection(t + "Database=timers;");
            //MySqlCommand cmd = new MySqlCommand("SELECT card, location, time FROM LocationPasses WHERE year = " + year.ToString() + " ORDER BY time", conn);

            //conn.Open();
            //var data = cmd.ExecuteReader();

            //while (data.Read())
            //{
            //    EmitData d = new EmitData()
            //    {
            //        Id = data.GetInt16("card"),
            //        BoxId = data.GetInt16("location"),
            //        Time = data.GetDateTime("time"),
            //        Force = false
            //    };
            //    race.AddPass(d);
            //}
            //data.Close();
            //conn.Close();

            race.InTestMode = false;
            return race;
        }

        public int ParticipantClassCount(String c)
        {
            return Participants.Where(p => p.Classes.Exists(pc => pc.Id.Equals(c))).Count();
        }

        public List<Result> Top10Leg(int timestation)
        {
            return Participants.Where(p => !Testers.Contains(p.EmitID)).SelectMany(p => p.Splits(p.Classes[0].Id)).Where(r => r.Location == timestation).OrderBy(r => r.Ticks).Take(10).ToList<Result>();
        }
    }
}
