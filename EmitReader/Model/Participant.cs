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
            TimeStamps = new SortedDictionary<TimeStation, DateTime>();
            Telephone = new List<String>();
        }

        public int Startnumber { get; set; }
        public int EmitID { get; set; }

        public String Name { get; set; }
        public List<String> Telephone { get; set; }

        public ParticipantClass Class { get; set; }
        public Boolean IsBusiness { get; set; }
        public Boolean IsTeam { get; set; }
        public Boolean IsSuper { get; set; }

        public List<String> TeamMembers { get; set; }

        public SortedDictionary<TimeStation, DateTime> TimeStamps { get; protected set; }

        public SortedDictionary<TimeStation, DateTime> OfficialTimeStamps
        {
            get
            {
                return TimeStamps.Where(key => key.Key.Official).ToSortedDictionary();
            }
        }
    }
}
