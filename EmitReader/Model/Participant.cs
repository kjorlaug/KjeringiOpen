using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmitReaderLib.Utils;

namespace EmitReaderLib.Model
{
    public class Participant
    {
        public Participant()
        {
            Telephone = new List<String>();
            TeamMembers = new List<String>();
            Classes = new List<ParticipantClass>();
            Comments = new List<String>();
            Passes = new Dictionary<int, EmitData>();
            LegSplits = new List<string>();
            _splits = new List<Result>();
        }

        public int Startnumber { get; set; }
        public int EmitID { get; set; }

        public String Name { get; set; }
        public List<String> Telephone { get; set; }

        public List<ParticipantClass> Classes { get; set; }
        public Boolean IsCompany { get; set; }
        public Boolean IsTeam { get; set; }
        public Boolean IsSuper { get; set; }

        public List<String> TeamMembers { get; set; }
        public String CompanyName { get; set; }
        public String Club { get; set; }

        public Boolean Star { get; set; }
        public List<String> Comments { get; set; }

        public Dictionary<int, EmitData> Passes { get; set; }

        public bool PassedTimestation(int id)
        {
            return Passes.ContainsKey(id);
        }

        public Result Leg(String classId, int location)
        {
            return Splits(classId).Where(r => r.Location == location).FirstOrDefault();
        }

        public List<Result> _splits { get; set; }

        public List<String> LegSplits { get; set; }

        public List<Boolean> LegEstimated { get; set; }

        public List<Result> Splits(String classId = "")
        {
            if (classId == "")
                return _splits;
            else
                return _splits.Where(r => r.ClassId.Equals(classId)).ToList<Result>();
        }

        public String TotalTime { get; set; }
        
        public TimeSpan EstimatedArrival { get; set; }
        public DateTime RealArrival { get; set; }
        public long EstimatedTicks { get { return EstimatedArrival.Ticks; } }
        public String EstimatedTime { get { return EstimatedArrival.ToString(@"hh\:mm\:ss"); } }

        public Boolean Finished { get; set; }

        public TimeSpan CurrentTime { get; internal set; }
    }
}