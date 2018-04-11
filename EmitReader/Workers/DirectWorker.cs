using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Workers
{
    public class DirectWorker : IWorker
    {
        public DirectWorker(Race race)
        {
            Race = race;
        }

        public event EventHandler<LogEventArgs> LogEntry;

        protected Race Race { get; set; }

        public void ProcessData(Model.EmitData data)        
        {
            try
            {
                Race.AddPass(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
