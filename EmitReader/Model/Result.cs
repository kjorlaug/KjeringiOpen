using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class Result 
    {
        public Boolean Official { get; set; }

        public String CurrentSplit
        {
            get {
                if (Splits == null)
                    return "";
                else
                    return Splits.Last();
            }
        }

        public String Total { get; set; }
        public String StationName { get; set; }

        public Dictionary<String, int> Positions { get; set; }

        public String Name { get; set; }
        public List<String> Telephone { get; set; }

        public int EmitID { get; set; }
        public int Startnumber { get; set; }
        public List<String> ParticipantClasses { get; set; }

        public List<String> TeamMembers { get; set; }
        public List<String> Splits { get; set; }

        public Boolean Start { get; set; }
        public List<String> Comments { get; set; }
    }
}
