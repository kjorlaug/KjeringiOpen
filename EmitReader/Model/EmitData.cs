using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Model
{
    public class EmitData : IComparable
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

        public Boolean Force { get;set; }

        public Boolean Test { 
            get {

                return false; //Testers.IndexOf(Id) >= 0;
            } 
        }

        public int CompareTo(object obj)
        {
            if (obj is EmitData)
            {
                return this.Time.CompareTo((obj as EmitData).Time);
            }
            else
                throw new NotImplementedException();
        }
    }
}
