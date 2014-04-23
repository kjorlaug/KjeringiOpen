using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Printing;
using Microsoft.AspNet.SignalR.Client;

namespace PrintConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            HubConnection myHubConn;
            IHubProxy myHub;
            
            myHubConn = new HubConnection("http://localhost:53130/");
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHub.On<String, String>("addNewMessageToPage", (name, message) =>
            {
                PrinterSettings ps = new PrinterSettings();
                RawPrinterHelper.SendStringToPrinter(ps.PrinterName, name + " " + message);
                Console.WriteLine("Printed");
            });

            myHubConn.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Console.WriteLine("Error opening connection", task.Exception.Message);
                else
                    Console.WriteLine("Connected");
            }).Wait();

            Console.Read();
            myHubConn.Stop();
            Console.WriteLine("Stopped");
        }
    }
}
