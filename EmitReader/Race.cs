using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using EmitReaderLib.Writers;

namespace EmitReaderLib
{
    public class Race
    {
        private volatile Boolean resultsVolatile;
        private object syncRoot = new Object();

        public Race(IRaceWriter writer)
        {
            Classes = new List<ParticipantClass>();
            Participants = new List<Participant>();
            TimeStations = new List<TimeStation>();
            ParticipantByEmit = new Dictionary<int, Participant>();
            ParticipantListByClass = new SortedDictionary<ParticipantClass, List<Participant>>(new ParticipantClassComparer());
            Passes = new List<EmitData>();
            Results = new List<Result>();
            resultsVolatile = false;
            this.writer = writer;
        }

        public String Name { get; set; }

        public List<ParticipantClass> Classes { get; protected set; }
        public List<Participant> Participants { get; protected set; }
        public List<TimeStation> TimeStations { get; protected set; }

        protected List<EmitData> Passes { get; set; }
        protected Dictionary<int, Participant> ParticipantByEmit { get; set; }
        protected SortedDictionary<ParticipantClass, List<Participant>> ParticipantListByClass { get; set; }

        protected List<Result> Results { get; set; }
        
        protected IRaceWriter writer { get; set; }

        public void AddParticipant(Participant p)
        {
            Participants.Add(p);

            foreach (ParticipantClass c in p.Classes)
            {
                if (!ParticipantListByClass.ContainsKey(c))
                    ParticipantListByClass.Add(c, new List<Participant>());

                ParticipantListByClass[c].Add(p);
            }
            ParticipantByEmit.Add(p.EmitID, p);
        }

        public Participant AddPass(EmitData emitdata) { 
            var participant = ParticipantByEmit[emitdata.Id];
            var timestation = TimeStations.Find(x => x.Id.Equals(emitdata.BoxId));

            writer.PersistPass(emitdata);

            lock (syncRoot)
            {
                // Force? Delete all passes by chip on that box
                if (emitdata.Force || emitdata.Test)
                {
                    resultsVolatile = emitdata.Force;
                    participant.Passes.Remove(timestation);
                    participant.Positions.Remove(timestation);
                }

                // Duplicate?
                if (!emitdata.Force && !emitdata.Test && timestation.Official && participant.Passes.ContainsKey(timestation))
                {
                    return null;
                }

                participant.Passes.Add(timestation, emitdata);

                var pos = new ParticipantDictionary();

                if (participant.Passes.Count > 1)
                {
                    foreach (ParticipantClass c in participant.Classes)
                        pos.Add(
                            c,
                            ParticipantListByClass[c]
                                .Where(p => p.Passes.ContainsKey(timestation))
                                .OrderBy(p => p.Passes[timestation].Time)
                                .ToList<Participant>()
                                .IndexOf(participant) + 1
                        );
                }
                
                participant.Positions.Add(timestation, pos);

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
                    ParticipantListByClass[c]
                        .Except(list)
                        .Where(p => p.Passes.ContainsKey(t))
                        .OrderBy(p => p.Passes[t].Time)
                        .ToList<Participant>()
                );
            }

            list.AddRange(
                ParticipantListByClass[c]
                    .Except(list)
                    .Where(p => p.Passes.Count == 1)
                    .OrderBy(p => p.Name)
                    .ToList<Participant>()
             );

            return list;
        }
    }
}
