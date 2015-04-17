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

            //EmitMonitor monitor = new EmitMonitor(new MySqlReader(120, "Kjeringi.2013", new List<String>() {"78", "70", "79", "248"}, "3", "5000", "9999") , new SignalWorker()); // 2013 data
            //EmitMonitor monitor = new EmitMonitor(new MySqlReader(120, "Kjeringi.2013", new List<String>() { "70", "71", "72", "73", "248" }, "3622", "6010", "9999"), new SignalWorker()); // 2014 data
            EmitMonitor monitor = new EmitMonitor(new TestReader(new List<int>() {1097, 1098, 1099}), new SignalWorker()); // Testers
            //EmitMonitor monitor = new EmitMonitor(new UsbSerialReader(), new SignalWorker()); // Live
            monitor.StartMonitoring();

            Console.ReadLine();
        }

    }
}
