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
            resultsVolatile = false;
        }

        public String Name { get; set; }

        public List<ParticipantClass> Classes { get; protected set; }
        public List<Participant> Participants { get; protected set; }
        public List<TimeStation> TimeStations { get; protected set; }

        public List<int> Testers { get; set; }

        protected List<EmitData> Passes { get; set; }
        protected Dictionary<int, Participant> ParticipantByEmit { get; set; }
        internal Dictionary<String, List<Participant>> ParticipantListByClass { get; set; }
        
        public void AddParticipant(Participant p)
        {
            p.Race = this;
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

            if (!ParticipantByEmit.ContainsKey(emitdata.Id))
                return null;

            var participant = ParticipantByEmit[emitdata.Id];
            var timestation = TimeStations.Find(x => x.Id.Equals(emitdata.BoxId));

            // going back in time?
            if (participant.Passes.Count > 0 && participant.Passes.Last().Value.Time > emitdata.Time)
                return null;

            lock (syncRoot)
            {
                // Force? Delete all passes by chip on that box
                if (emitdata.Force || emitdata.Test)
                {
                    try
                    {
                        participant.Passes.Remove(timestation.Id);
                    }
                    catch (Exception ex) { }
                }

                // Duplicate?
                if (!emitdata.Force && !emitdata.Test && timestation.Official && participant.Passes.ContainsKey(timestation.Id))
                {
                    return null;
                }

                participant.Passes.Add(timestation.Id, emitdata);

                DateTime startTime = participant.Passes.First().Value.Time;
                TimeSpan totalTime = (emitdata.Time - startTime);

                if (!participant.Finished && !timestation.Start)
                {
                    participant.TotalTime = totalTime.ToString(@"hh\:mm\:ss");

                    if (timestation.Progress.HasValue)
                    {
                        long estimate = Convert.ToInt64((totalTime.Ticks / timestation.Progress.Value) * 100);
                        participant.EstimatedArrival = startTime + TimeSpan.FromTicks(estimate);
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
    }
}
