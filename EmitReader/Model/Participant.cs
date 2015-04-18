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
            Positions = new Dictionary<int, Dictionary<String, int>>();
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
        public String CompanyName { get; set;
        }
        public Boolean Star { get; set; }
        public List<String> Comments { get; set; }

        public Dictionary<int, EmitData> Passes { get; set; }
        public Dictionary<int, Dictionary<String, int>> Positions { get; set; }

        public List<Result> Splits {get;set;}

        public String TotalTime { get; set; }
        public DateTime EstimatedArrival { get; set; }
        public DateTime RealArrival { get; set; }
        public Boolean Finished { get; set; }

    }
}