using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class Race
    {
        public Race()
        {
            Classes = new List<ParticipantClass>();
            Participants = new List<Participant>();
            TimeStations = new List<TimeStation>();
            ParticipantByEmit = new Dictionary<int, Participant>();
            ParticipantListByClass = new SortedDictionary<ParticipantClass, List<Participant>>();
        }

        public String Name { get; set; }

        public List<ParticipantClass> Classes { get; protected set; }
        public List<Participant> Participants { get; protected set; }
        public List<TimeStation> TimeStations { get; protected set; }

        public Dictionary<int, Participant> ParticipantByEmit { get; protected set; }
        public SortedDictionary<ParticipantClass, List<Participant>> ParticipantListByClass { get; protected set; }

        public void AddParticipant(Participant p)
        {
            Participants.Add(p);

            if (!ParticipantListByClass.ContainsKey(p.Class))
                ParticipantListByClass.Add(p.Class, new List<Participant>());

            ParticipantListByClass[p.Class].Add(p);
            ParticipantByEmit.Add(p.EmitID, p);
        }
    }
}
