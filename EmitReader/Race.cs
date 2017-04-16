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

namespace EmitReaderLib
{
    public class Race
    {
        private volatile Boolean resultsVolatile;
        private object syncRoot = new Object();

        public Race()
        {
            Classes = new List<ParticipantClass>();
            Participants = new List<Participant>();
            TimeStations = new List<TimeStation>();
            ParticipantByEmit = new Dictionary<int, Participant>();
            ParticipantListByClass = new Dictionary<String, List<Participant>>();
            Passes = new List<EmitData>();
            resultsVolatile = false;
            Testers = new List<int>();
        }

        public String Name { get; set; }

        public List<ParticipantClass> Classes { get; protected set; }
        public List<Participant> Participants { get; protected set; }
        public List<TimeStation> TimeStations { get; protected set; }

        public List<int> Testers { get; set; }

        protected List<EmitData> Passes { get; set; }
        protected Dictionary<int, Participant> ParticipantByEmit { get; set; }
        internal Dictionary<String, List<Participant>> ParticipantListByClass { get; set; }
        public Boolean InTestMode { get; set; }

        public void AddParticipant(Participant p)
        {
            Participants.Add(p);

            IndexParticipant(p);
        }

        protected void IndexParticipant(Participant p)
        {
            foreach (ParticipantClass c in p.Classes)
            {
                if (!ParticipantListByClass.ContainsKey(c.Id))
                    ParticipantListByClass.Add(c.Id, new List<Participant>());

                ParticipantListByClass[c.Id].Add(p);
            }
            ParticipantByEmit.Add(p.EmitID, p);
        }

        public Participant AddPass(EmitData emitdata) {

            //if (!ParticipantByEmit.ContainsKey(emitdata.Id))
            //    return null;

            var participant = ParticipantByEmit[emitdata.Id];
            var timestation = TimeStations.Find(x => x.Id.Equals(emitdata.BoxId));

            //// going back in time?
            //if (participant.Passes.Count > 0 && participant.Passes.Last().Value.Time > emitdata.Time)
            //    return null;

            lock (syncRoot)
            {
                if (!participant.Passes.ContainsKey(timestation.Id))
                    participant.Passes.Add(timestation.Id, emitdata);
                else if (participant.Passes[timestation.Id].Estimated || emitdata.Force)
                    participant.Passes[timestation.Id] = emitdata;

                DateTime startTime = participant.Passes.First().Value.Time;
                TimeSpan totalTime = (emitdata.Time - startTime);

                if (!participant.Finished && !timestation.Start && timestation.Progress.HasValue)
                {
                    double ticksSoFar = (double)totalTime.Ticks;
                    double estimate = (ticksSoFar / timestation.Progress.Value) * 100;

                    participant.CurrentTime = totalTime;
                    participant.TotalTime = totalTime.ToString(@"hh\:mm\:ss");
                    participant.EstimatedArrival = TimeSpan.FromTicks((long)estimate);

                    // missing values?
                    foreach (TimeStation shouldHavePassed in TimeStations.Where(t => t.Sequence < timestation.Sequence && t.Official && t.Progress.HasValue))
                        if (!participant.Passes.ContainsKey(shouldHavePassed.Id))
                        {
                            participant.Passes.Add(
                                shouldHavePassed.Id,
                                new EmitData() {
                                    Estimated = true,
                                    BoxId = shouldHavePassed.Id,
                                    Chip = emitdata.Chip,
                                    Id = emitdata.Id,
                                    Voltage = emitdata.Voltage,
                                    Time = startTime + TimeSpan.FromTicks((long)(estimate * (shouldHavePassed.Progress / 100)))
                                });
                        }
                    }

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
                                        Total = participant.TotalTime,
                                        Position = this.ParticipantListByClass[c.Id]
                                            .Where(p => p.Passes.ContainsKey(cur.Key))
                                            .OrderBy(p => p.Passes[cur.Key].Time)
                                            .ToList<Participant>()
                                            .IndexOf(participant) + 1

                                    }
                                        ).ToList<Result>());
                    }

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
            var c = Classes.First(p => p.Id.Equals(participantClassId));
            var list = new List<Participant>();

            if (!ParticipantListByClass.ContainsKey(c.Id))
                return new List<Participant>();

            foreach (TimeStation t in TimeStations.Where(ts => ts.Official && ! ts.Start).OrderByDescending(ts => ts.Sequence))
            {
                list.AddRange(
                    ParticipantListByClass[c.Id]
                        .Except(list)
                        .Where(p => p.Passes.ContainsKey(t.Id))
                        .OrderBy(p => p.Passes[t.Id].Time)
                        .ToList<Participant>()
                );
            }

            list.AddRange(
                ParticipantListByClass[c.Id]
                    .Except(list)
                    .Where(p => p.Passes.Count == 1)
                    .OrderBy(p => p.Startnumber)
                    .ToList<Participant>()
             );

            return list;
        }

        public void Initialize()
        {
            foreach (Participant p in Participants)
                IndexParticipant(p);
        }
        
        public static Race LoadYear(int year, String jsonFile)
        {
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
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2015("kjeringi", new List<int>(), "2015")).BuildRace(race);
                    break;
                case 2016:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2015("kjeringi.2016", Enumerable.Range(5001, 29).ToList<int>(), "2016")).BuildRace(race);
                    break;
                case 2017:
                    (new EmitReaderLib.Builders.MySqlRaceBuilder2015("kjeringi.2017", Enumerable.Range(5001, 29).ToList<int>(), "2017")).BuildRace(race);
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

        public int ParticipantClassCount(String c)
        {
            if (ParticipantListByClass.ContainsKey(c))
                return ParticipantListByClass[c].Count;
            else
                return 0;
        }

        public List<Result> Top10Leg(int timestation)
        {
            return Participants.Where(p => !Testers.Contains(p.EmitID)).SelectMany(p => p.Splits(p.Classes[0].Id)).Where(r => r.Location == timestation).OrderBy(r => r.Ticks).Take(10).ToList<Result>();
        }
    }
}
