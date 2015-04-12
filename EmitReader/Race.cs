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
        public Race(IRaceWriter writer)
        {
            Classes = new List<ParticipantClass>();
            Participants = new List<Participant>();
            TimeStations = new List<TimeStation>();
            ParticipantByEmit = new Dictionary<int, Participant>();
            ParticipantListByClass = new SortedDictionary<ParticipantClass, List<Participant>>();
            Passes = new List<Pass>();
            this.writer = writer;
        }

        public String Name { get; set; }

        public List<ParticipantClass> Classes { get; protected set; }
        public List<Participant> Participants { get; protected set; }
        public List<TimeStation> TimeStations { get; protected set; }

        protected List<Pass> Passes { get; set; }
        protected Dictionary<int, Participant> ParticipantByEmit { get; set; }
        protected SortedDictionary<ParticipantClass, List<Participant>> ParticipantListByClass { get; set; }
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
            
            var pass = new Pass {Station = timestation, EmitID = emitdata.Id, Time = emitdata.Time};

            // Duplicate?
            if (!emitdata.Test && timestation.Official && Passes.Contains(pass))
            {
                if (Passes[Passes.IndexOf(pass)].Time < pass.Time)
                    throw new Exception("Duplicate pass");
            }

            participant.TimeStamps.Add(timestation, emitdata.Time);
            Passes.Add(pass);
            writer.PersistPass(pass);

            return BuildResult(pass, timestation, participant);
        }

        public Result BuildResult(Pass pass, TimeStation timestation, Participant participant) {
            var result = new Result {
                Official = timestation.Official,
                StationName = timestation.Name,
                Name = participant.Name,
                Telephone = participant.Telephone,
                EmitID = participant.EmitID,
                Startnumber = participant.Startnumber,
                TeamMembers = participant.TeamMembers,
                ParticipantClasses = participant.Classes.Select(c => c.Name).ToList<String>()
            };

            DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 14, 0);
            result.Splits = new List<String>();
            TimeSpan total = new TimeSpan(0);

            // Find all splits
            foreach (DateTime split in participant.OfficialTimeStamps.Values) {
                result.Splits.Add((split - start).ToString(@"hh\:mm\:ss"));
                total += (split - start);
                start = split;
            }
            result.Total = total.ToString(@"hh\:mm\:ss");

            result.Positions = new Dictionary<String, int>();
            foreach (ParticipantClass c in participant.Classes)
            {
                result.Positions.Add(
                    c.Name,
                    ParticipantListByClass[c]
                        .Where(p => p.OfficialTimeStamps.ContainsKey(timestation))
                        .OrderBy(p => p.OfficialTimeStamps[timestation])
                        .ToList<Participant>()
                        .IndexOf(participant) + 1
                    );
            }
                        
            return result;
        }    
    }
}
