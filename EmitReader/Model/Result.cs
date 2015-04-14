﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class Result 
    {
        public String CurrentSplit
        {
            get {
                if (Splits == null)
                    return "";
                else
                    return Splits.Last();
            }
        }

        public String TotalString { get; set; }
        public TimeSpan Total { get; set; }
        public TimeStation TimeStation { get; set; }

        public Dictionary<String, int> Positions { get; set; }

        public String Name { get; set; }
        public List<String> Telephone { get; set; }

        public int EmitID { get; set; }
        public int Startnumber { get; set; }
        public List<ParticipantClass> ParticipantClasses { get; set; }

        public List<String> TeamMembers { get; set; }
        public List<String> Splits { get; set; }

        public Boolean Start { get; set; }
        public List<String> Comments { get; set; }

        public void CalculatePositions(SortedDictionary<ParticipantClass, List<Participant>> ParticipantListByClass, Participant participant)
        {
            this.Positions = new Dictionary<String, int>();

            foreach (ParticipantClass c in ParticipantClasses)
            {
                this.Positions.Add(
                    c.Name,
                    ParticipantListByClass[c]
                        .Where(p => p.OfficialTimeStamps.ContainsKey(this.TimeStation))
                        .OrderBy(p => p.OfficialTimeStamps[this.TimeStation])
                        .ToList<Participant>()
                        .IndexOf(participant) + 1
                    );
            }
        }
    }
}
