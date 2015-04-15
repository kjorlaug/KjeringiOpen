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
            Passes = new PassesDictionary(new TimestationComparer());
            Positions = new PositionDictionary(new TimestationComparer());
        }

        public int Startnumber { get; set; }
        public int EmitID { get; set; }

        public String Name { get; set; }
        public List<String> Telephone { get; set; }

        public List<ParticipantClass> Classes { get; set; }
        public Boolean IsBusiness { get; set; }
        public Boolean IsTeam { get; set; }
        public Boolean IsSuper { get; set; }

        public List<String> TeamMembers { get; set; }

        public Boolean Star { get; set; }
        public List<String> Comments { get; set; }

        public PassesDictionary Passes { get; set; }
        public PositionDictionary Positions { get; set; }

        public List<Result> Splits
        {
            get
            {
                var res = new List<Result>();
                if (Passes.Count > 1)
                    foreach (ParticipantClass c in this.Classes)
                        res.AddRange(
                            Passes
                                .Where(p => p.Key.Official)
                                .OrderBy(p => p.Key.Sequence)
                                .SelectWithPrevious((prev, cur) =>
                                    new Result() { 
                                        Sequence = cur.Key.Sequence, 
                                        Leg = cur.Key.Name, 
                                        Class = c.Name, 
                                        Name = this.IsSuper ? "" : TeamMember(cur.Key), 
                                        Time = (cur.Value.Time - prev.Value.Time).ToString(@"hh\:mm\:ss"), 
                                        Position = this.Positions[cur.Key].ContainsKey(c) ? this.Positions[cur.Key][c] : 99 }
                                        )
                                .ToList<Result>());
                return res;
            }
        }

        protected String TeamMember(TimeStation leg)
        {
            return "Rune";
        }

    }


    [Newtonsoft.Json.JsonArray]
    public class PassesDictionary : SortedDictionary<TimeStation, EmitData> {
        public PassesDictionary(IComparer<TimeStation> i) : base(i) { }
    }

    [Newtonsoft.Json.JsonArray]
    public class PositionDictionary : SortedDictionary<TimeStation, ParticipantDictionary> {
        public PositionDictionary(IComparer<TimeStation> i) : base(i) { }
    }

    [Newtonsoft.Json.JsonArray]
    public class ParticipantDictionary : Dictionary<ParticipantClass, int> { }
}
