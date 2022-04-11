using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using EmitReaderLib.Utils;
using Newtonsoft.Json;

namespace EmitReaderLib.Model
{
    public class Participant : IDisposable
    {
        public Participant()
        {
            Passes = new Dictionary<int, EmitData>();
        }

        public int Id { get; set; }
        public DateTime Updated { get; set; }

        public int Startnumber { get; set; }
        public int EmitID { get; set; }

        public String Name { get; set; }
        public List<String> Telephone { get; set; }

        public List<ParticipantClass> Classes { get; set; }
        public Boolean IsCompany { get; set; }
        public Boolean IsTeam { get; set; }
        public Boolean IsSuper { get; set; }

        public List<int> Ages { get; set; }

        public List<String> TeamMembers { get; set; }
        public List<String> ShirtSizes { get; set; }
        public String CompanyName { get; set; }
        public String Club { get; set; }

        public List<int> Gender { get; set; }

        public Boolean Star { get; set; }

        [ScriptIgnore]
        public Dictionary<int, EmitData> Passes { get; set; }

        public bool PassedTimestation(int id)
        {
            return Passes.ContainsKey(id);
        }

        public Result Leg(String classId, int location)
        {
            return Splits(classId).Where(r => r.Location == location).FirstOrDefault();
        }

        [ScriptIgnore]
        public List<Result> _splits { get; set; }

        [ScriptIgnore]
        public List<String> LegSplits { get; set; }

        [ScriptIgnore]
        public List<Boolean> LegEstimated { get; set; }

        public List<Result> Splits(String classId = "")
        {
            if (classId == "")
                return _splits;
            else
                return _splits.Where(r => r.ClassId.Equals(classId)).ToList<Result>();
        }

        public void Dispose()
        {
            Passes = null;
            _splits = null;
            TeamMembers = null;
            Telephone = null;
            ShirtSizes = null;
        }

        [ScriptIgnore]
        public String TotalTime { get; set; }

        [ScriptIgnore]
        public TimeSpan EstimatedArrival { get; set; }
        [ScriptIgnore]
        public DateTime RealArrival { get; set; }
        [ScriptIgnore]
        public long EstimatedTicks { get { return EstimatedArrival.Ticks; } }
        [ScriptIgnore]
        public String EstimatedTime { get { return EstimatedArrival.ToString(@"hh\:mm\:ss"); } }

        [ScriptIgnore]
        public Boolean Finished { get; set; }

        [ScriptIgnore]
        public TimeSpan CurrentTime { get; internal set; }

        [ScriptIgnore]
        public Race Race { get; set; }
        public bool ShouldSerializeRace() { return false; }
    }
}