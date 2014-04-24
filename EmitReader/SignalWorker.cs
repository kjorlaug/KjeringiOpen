using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace EmitReaderLib
{
    public class SignalWorker : IWorker, IDisposable
    {

        private HubConnection myHubConn;
        private IHubProxy myHub;

        public SignalWorker()
        {
            String hub = ConfigurationManager.AppSettings["hub"];
            String name = ConfigurationManager.AppSettings["name"];

            myHubConn = new HubConnection(hub);
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHubConn.Start()
                .ContinueWith((prevTask) => {
                    myHub.Invoke("Join", "EmitReader - " + name);
                }).Wait();
        }

        public void ProcessData(EmitData data)
        {
            myHub.Invoke("SendPassering", new object[] { data }).ContinueWith(task =>
                 {
                     if (task.IsFaulted)
                     {
                         Console.WriteLine("An error occurred during the method call {0}", task.Exception.GetBaseException());
                     }
                     else
                     {
                         Console.WriteLine("Successfully called MethodOnServer");
                     }
                 });
        }

        public void Dispose()
        {
            myHubConn.Stop();
        }
    }
}
