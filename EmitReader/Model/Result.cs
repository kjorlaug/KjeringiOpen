using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class Result 
    {
        public String Class { get; set; }
        public int Sequence { get; set; }
        public int Location { get; set; }
        public int Startnumber { get; set; }
        public String Leg { get; set; }
        public String Name { get; set; }
        public String Team { get; set; }
        public String Time { get; set; }
        public String Total { get; set; }
        public long Ticks { get; set; }
        public int Position { get; set; }
        public int EmitId { get; set; }
    }
}
