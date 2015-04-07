using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class ParticipantClass : IComparable<ParticipantClass>
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public int Sequence { get; set; }

        public int CompareTo(ParticipantClass other)
        {
            return this.Sequence.CompareTo(other.Sequence);
        }
    }
}
