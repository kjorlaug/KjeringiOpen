using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using EmitReaderLib.Writers;
using EmitReaderLib.Utils;

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
            Results = new List<Result>();
            resultsVolatile = false;
        }

        public String Name { get; set; }

        public List<ParticipantClass> Classes { get; protected set; }
        public List<Participant> Participants { get; protected set; }
        public List<TimeStation> TimeStations { get; protected set; }

        protected List<EmitData> Passes { get; set; }
        protected Dictionary<int, Participant> ParticipantByEmit { get; set; }
        protected Dictionary<String, List<Participant>> ParticipantListByClass { get; set; }

        protected List<Result> Results { get; set; }
        
        public void AddParticipant(Participant p)
        {
            Participants.Add(p);

            foreach (ParticipantClass c in p.Classes)
            {
                if (!ParticipantListByClass.ContainsKey(c.Id))
                    ParticipantListByClass.Add(c.Id, new List<Participant>());

                ParticipantListByClass[c.Id].Add(p);
            }
            ParticipantByEmit.Add(p.EmitID, p);
        }

        public Participant AddPass(EmitData emitdata) {
            if (!ParticipantByEmit.ContainsKey(emitdata.Id))
                return null;

            var participant = ParticipantByEmit[emitdata.Id];
            var timestation = TimeStations.Find(x => x.Id.Equals(emitdata.BoxId));

            lock (syncRoot)
            {
                // Force? Delete all passes by chip on that box
                if (emitdata.Force || emitdata.Test)
                {
                    resultsVolatile = emitdata.Force;
                    participant.Passes.Remove(timestation.Id);
                    participant.Positions.Remove(timestation.Id);
                }

                // Duplicate?
                if (!emitdata.Force && !emitdata.Test && timestation.Official && participant.Passes.ContainsKey(timestation.Id))
                {
                    return null;
                }

                participant.Passes.Add(timestation.Id, emitdata);

                var recalcFor = new List<Participant>();

                if (resultsVolatile)
                {
                    foreach (ParticipantClass c in participant.Classes)
                        recalcFor.AddRange(
                            ParticipantListByClass[c.Id].Except(recalcFor)
                            );
                }
                else
                    recalcFor.Add(participant);


                foreach (Participant par in recalcFor)
                {
                    var pos = new Dictionary<String, int>();
                    var res = new List<Result>();

                    if (par.Passes.Count > 1)
                    {
                        foreach (ParticipantClass c in par.Classes)
                        {
                            pos.Add(
                                c.Id,
                                ParticipantListByClass[c.Id]
                                    .Where(p => p.Passes.ContainsKey(timestation.Id))
                                    .OrderBy(p => p.Passes[timestation.Id].Time)
                                    .ToList<Participant>()
                                    .IndexOf(participant) + 1
                            );
                        }
                    }

                    par.Positions.Add(timestation.Id, pos);

                    if (par.Passes.Count > 1)
                        foreach (ParticipantClass c in par.Classes)
                        {
                            res.AddRange(
                                par.Passes
                                    .Where(p => TimeStations.Find(ts => ts.Id.Equals(p.Key)).Official)
                                    .OrderBy(p => p.Value.Time)
                                    .SelectWithPrevious((prev, cur) =>
                                        new Result()
                                        {
                                            Sequence = TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Sequence,
                                            Leg = TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Name,
                                            Class = c.Name,
                                            Name = par.IsSuper ? "" : par.TeamMembers[TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Sequence - 1],
                                            Time = (cur.Value.Time - prev.Value.Time).ToString(@"hh\:mm\:ss"),
                                            Position = par.Positions[cur.Key][c.Id]
                                        }
                                            )
                                    .ToList<Result>());
                        }

                    participant.Splits = res;

                    DateTime startTime = participant.Passes.First().Value.Time;
                    TimeSpan totalTime = (emitdata.Time - startTime);

                    if (!participant.Finished && !timestation.Start )
                    {
                        participant.TotalTime = totalTime.ToString(@"hh\:mm\:ss");

                        if (timestation.Progress.HasValue)
                        {
                            long estimate = Convert.ToInt64((totalTime.Ticks / timestation.Progress.Value)*100);
                            participant.EstimatedArrival = startTime + TimeSpan.FromTicks(estimate);
                        }
                    }

                    if (timestation.Finish)
                    {
                        participant.Finished = true;
                        participant.RealArrival = emitdata.Time;
                    }

                }
                return participant;
            }
        }

        public ICollection<Participant> GetResults(String participantClassId)
        {
            var c = Classes.First(p => p.Id.Equals(participantClassId));
            var list = new List<Participant>();

            foreach (TimeStation t in TimeStations.Where(ts => ts.Official).Skip(1).OrderByDescending(ts => ts.Sequence))
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
                    .OrderBy(p => p.Name)
                    .ToList<Participant>()
             );

            return list;
        }
    }
}
