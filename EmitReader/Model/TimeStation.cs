using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class TimeStation
    {
        public int Id { get; set; }

        public Boolean Official { get; set; }
        public Boolean Start { get; set; }
        public Boolean Finish { get; set; }
        public String Name { get; set; }
        public int Sequence { get; set; }
        public Double? Progress { get; set; }
        public int Passes { get; set; }
    }

    public class TimestationComparer : Comparer<TimeStation>
    {
        public override int Compare(TimeStation x, TimeStation y)
        {
            return x.Id - y.Id;
        }
    }
}
