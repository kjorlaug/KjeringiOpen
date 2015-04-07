using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class TimeStation : IComparable<TimeStation>
    {
        public int Id { get; set; }

        public Boolean Official { get; set; }
        public String Name { get; set; }
        public int Sequence { get; set; }

        public int CompareTo(TimeStation other)
        {
            return this.Sequence.CompareTo(other.Sequence);
        }
    }
}
