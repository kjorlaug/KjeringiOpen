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

            EmitMonitor monitor = null;

            String arg = "";
            if (args.Length > 0)
                arg = args[0];

            switch (arg)
            {
                case "sticksignal":
                    monitor = new EmitMonitor(new Com4UsbSerialReader(), new SignalWorker());
                    break;
                case "signal":
                    monitor = new EmitMonitor(new UsbSerialReader(), new SignalWorker());
                    break;
                default:
                    //monitor = new EmitMonitor(new Com4UsbSerialReader(), new SignalWorker());
                    //monitor = new EmitMonitor(new UsbSerialReader(), new SubmitWorker("http://ko.hoo9.com/timer/register"));
                    monitor = new EmitMonitor(new MySqlReader(), new SignalWorker());
                    break;
            }

            monitor.StartMonitoring();

            Console.ReadLine();
        }

    }
}
