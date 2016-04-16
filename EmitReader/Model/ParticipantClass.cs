using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class ParticipantClass
    {
        public ParticipantClass()
        {
            Classes = new List<String>();
            Official = true;
        }

        public String Id { get; set; }
        public String Name { get; set; }
        public int Sequence { get; set; }

        public Boolean Company { get; set; }
        public List<String> Classes { get; set; }
        public Boolean Official { get; set; }

        public Boolean Test { get; set; }

    }

    public class ParticipantClassComparer : Comparer<ParticipantClass>
    {
        public override int Compare(ParticipantClass x, ParticipantClass y)
        {
            if (x.Id.Equals(y.Id))
                return 0;
            else
                return x.Sequence - y.Sequence;

        }
    }
}
