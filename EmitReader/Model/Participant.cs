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
            //Positions = new Dictionary<int, Dictionary<String, int>>();
        }

        internal Race Race { get; set; }

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

        public Boolean Star { get; set; }
        public List<String> Comments { get; set; }

        public Dictionary<int, EmitData> Passes { get; set; }             

        //public List<Result> Splits {get;set;}
        public Dictionary<int, Dictionary<String, int>> Positions { get; set; }

        public List<Result> Splits
        {
            get
            {
                List<Result> res = new List<Result>();

                foreach (ParticipantClass c in this.Classes)
                {
                    res.AddRange(
                        this.Passes
                            .Where(p => Race.TimeStations.Find(ts => ts.Id.Equals(p.Key)).Official)
                            .OrderBy(p => p.Value.Time)
                            .SelectWithPrevious((prev, cur) =>
                                new Result()
                                {
                                    Class = c.Name,
                                    EmitId = this.EmitID,
                                    Sequence = Race.TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Sequence,
                                    Location = cur.Key,
                                    Leg = Race.TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Name,
                                    Name = this.IsSuper ? this.Name : this.TeamMembers[Race.TimeStations.Find(ts => ts.Id.Equals(cur.Key)).Leg - 1],
                                    Team = this.IsSuper ? "" : this.Name,
                                    Time = (cur.Value.Time - prev.Value.Time).ToString(@"hh\:mm\:ss"),
                                    Ticks = (cur.Value.Time - prev.Value.Time).Ticks,
                                    Total = this.TotalTime,
                                    Position = Race.ParticipantListByClass[c.Id]
                                        .Where(p => p.Passes.ContainsKey(cur.Key))
                                        .OrderBy(p => p.Passes[cur.Key].Time)
                                        .ToList<Participant>()
                                        .IndexOf(this) + 1

                                }
                                    )
                            .ToList<Result>());
                }
                return res;
            }
        }

        public String TotalTime { get; set; }
        
        public DateTime EstimatedArrival { get; set; }
        public DateTime RealArrival { get; set; }
        
        public Boolean Finished { get; set; }

    }
}