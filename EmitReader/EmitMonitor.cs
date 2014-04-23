using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib
{
    public class EmitMonitor : IDisposable
    {

        protected IEmitReader reader;
        protected IWorker worker;
        protected bool isMonitoring = false;

        public EmitMonitor(IEmitReader r, IWorker w) {
            reader = r;
            worker = w;
        }

        public void StartMonitoring()
        {
            if (reader == null)
                throw new NullReferenceException("No reader");

            reader.DataReceived += reader_DataReceived;
            reader.Start();
            isMonitoring = true;
            Console.WriteLine("Started");
        }

        public void StopMonitoring()
        {
            reader.Stop();
            isMonitoring = false;

        }

        void IDisposable.Dispose()
        {
            if (isMonitoring)
                reader.Stop();
        }

        protected void reader_DataReceived(object sender, EmitDataRecievedEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToLongTimeString() + ": DataRead ");
            worker.ProcessData(e.Data);
        }
    }
}
