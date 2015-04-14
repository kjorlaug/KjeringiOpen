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
            ParticipantListByClass = new SortedDictionary<ParticipantClass, List<Participant>>();
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

        public Result AddPass(EmitData emitdata) { 
            var participant = ParticipantByEmit[emitdata.Id];
            var timestation = TimeStations.Find(x => x.Id.Equals(emitdata.BoxId));

            writer.PersistPass(emitdata);

            // Force? Delete all passes by chip on that box
            if (emitdata.Force)
            {
                lock (syncRoot)
                {
                    resultsVolatile = true;
                    Passes.RemoveAll(p => p.Id.Equals(emitdata.Id) && p.BoxId.Equals(emitdata.BoxId));
                    participant.TimeStamps.Remove(timestation);
                    Results.RemoveAll(r => r.EmitID.Equals(emitdata.Id) && r.TimeStation.Id.Equals(timestation.Id));
                }
            }
            
            // Duplicate?
            if (!emitdata.Test && timestation.Official && Passes.Contains(emitdata))
            {
                if (Passes[Passes.IndexOf(emitdata)].Time < emitdata.Time)
                    throw new Exception("Duplicate pass");
            }

            participant.TimeStamps.Add(timestation, emitdata.Time);
            Passes.Add(emitdata);
                       
            var result = BuildResult(emitdata, timestation, participant);
            result.CalculatePositions(ParticipantListByClass, participant);

            Results.Add(result);

            // If results are volatile, we need to recalc positions on affected
            if (resultsVolatile) {
                lock (syncRoot)
                {
                    // fixed?
                    if (resultsVolatile)
                    {
                        foreach (Result res in Results.Where(r => r.Total.CompareTo(result.Total) > 0))
                            res.CalculatePositions(ParticipantListByClass, ParticipantByEmit[res.EmitID]);
                    }
                    resultsVolatile = false;
                }
            }
            return result;
        }

        protected Result BuildResult(EmitData pass, TimeStation timestation, Participant participant) {
            var result = new Result {
                TimeStation = timestation,
                Name = participant.Name,
                Telephone = participant.Telephone,
                EmitID = participant.EmitID,
                Startnumber = participant.Startnumber,
                TeamMembers = participant.TeamMembers,
                ParticipantClasses = participant.Classes
            };

            DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 14, 0);
            result.Splits = new List<String>();
            result.Total = new TimeSpan(0);

            // Find all splits
            foreach (DateTime split in participant.OfficialTimeStamps.Values) {
                result.Splits.Add((split - start).ToString(@"hh\:mm\:ss"));
                result.Total += (split - start);
                start = split;
            }
            result.TotalString = result.Total.ToString(@"hh\:mm\:ss");
            
            return result;
        }    

    }
}
