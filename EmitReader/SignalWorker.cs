using System;
using System.Collections.Generic;
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
            myHubConn = new HubConnection("http://localhost:53130/");
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");
            myHubConn.Start().Wait();
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
