using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib
{
    public class EmitData
    {
        public EmitData() {}
        public EmitData(int id, int chip, DateTime time, double voltage)
        {
            this.Id = id;
            this.Chip = chip;
            this.Time = time;
            this.Voltage = voltage;
            
        }

        public int Id { get; set; }
        public int BoxId { get; set; }
        public int Chip { get; set; }
        public DateTime Time {get; set;}
        public double Voltage { get; set; }
        public long Position { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String TeamName { get; set; }
        public String ClassCode { get; set; }
        public int Leg { get; set; }
        public TimeSpan LegTime { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
