    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EmitReaderLib;
using System.IO.Ports;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            EmitMonitor monitor = new EmitMonitor(new MySqlReader() , new SignalWorker());
            //EmitMonitor monitor = new EmitMonitor(new UsbSerialReader(), new SignalWorker());
            monitor.StartMonitoring();

            Console.ReadLine();
        }

    }
}
