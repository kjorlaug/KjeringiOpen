using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

using EmitReaderLib.Model;

namespace EmitReaderLib
{
    public class SignalWorker : IWorker, IDisposable
    {

        private HubConnection myHubConn;
        private IHubProxy myHub;

        private static Queue<EmitData> ToSend = new Queue<EmitData>();
        private static Boolean sending = false;
        private static Object lockObj = new Object();
        
        public SignalWorker()
        {
            String hub = ConfigurationManager.AppSettings["hub"];
            String name = ConfigurationManager.AppSettings["name"];

            myHubConn = new HubConnection(hub);
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHubConn.Reconnected += myHubConn_Reconnected;
            myHubConn.Reconnecting += myHubConn_Reconnecting;
            myHubConn.ConnectionSlow += myHubConn_ConnectionSlow;
            myHubConn.Closed += myHubConn_Closed;

            myHubConn.Start()
                .ContinueWith((prevTask) =>
                {
                    myHub.Invoke("Join", "EmitReader - " + name);
                }).Wait();
        }

        public void ProcessData(EmitData data)
        {
            lock (lockObj)
            {
                ToSend.Enqueue(data);
            }

            if (sending)
            {
                return;
            }

            lock (lockObj) { 
                sending = true;
            }

            while (ToSend.Count > 0)
            {
                EmitData dataToSend = ToSend.Peek();
                try
                {
                    // Fake dropouts
                    if (dataToSend.BoxId != 248 && (new Random()).Next(10) == 1)
                    {
                        Console.WriteLine("Skipped " + dataToSend.Id.ToString());
                        lock (lockObj)
                            ToSend.Dequeue();
                    }
                    else {
                        myHub.Invoke("SendPassering", new object[] { dataToSend });
                        lock (lockObj)
                            ToSend.Dequeue();
                        Console.WriteLine("Successfully " + dataToSend.Id.ToString());
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Error - sleeping 3 sec before retry");
                    System.Threading.Thread.Sleep(3000);
                }
            }
            lock (lockObj)
            {
                sending = false;
            }
        }

        void myHubConn_Closed()
        {
            Console.WriteLine("myHubConn_Closed New State - retrying in 5 sec:" + myHubConn.State);
            System.Threading.Thread.Sleep(5000);
            myHubConn.Start().Wait();
        }

        void myHubConn_ConnectionSlow()
        {
            Console.WriteLine("myHubConn_ConnectionSlow New State:" + myHubConn.State);
        }

        void myHubConn_Reconnecting()
        {
            Console.WriteLine("myHubConn_Reconnecting New State:" + myHubConn.State);
        }

        void myHubConn_Reconnected()
        {
            Console.WriteLine("myHubConn_Reconnected New State:" + myHubConn.State);
        }

        public void Dispose()
        {
            myHubConn.Stop();
        }
    }
}
