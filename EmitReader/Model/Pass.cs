using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class Pass : IComparable<Pass>
    {
        public TimeStation Station { get; set; }
        public int EmitID { get; set; }
        public DateTime Time { get; set; }

        public int CompareTo(Pass other)
        {
            if (Station.Id.Equals(other.Station.Id) && EmitID.Equals(other.EmitID))
                return 0;
            else
                return EmitID - other.EmitID;
        }
    }
}
