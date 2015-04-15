using EmitReaderLib.Model;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ResultsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            String hub = ConfigurationManager.AppSettings["hub"];

            HubConnection myHubConn;
            IHubProxy myHub;

            myHubConn = new HubConnection(hub);
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHubConn.Error += ex => Console.WriteLine("SignalR error: {0}", ex.Message, ex.InnerException);

            myHubConn.TraceLevel = TraceLevels.None;
            myHubConn.TraceWriter = Console.Out;

            myHubConn.Start()
                .ContinueWith((prevTask) =>
                {
                    myHub.Invoke("Join", "ResultService");
                    myHub.Invoke("AddtoGroup", "");
                }).Wait();

            myHub.On<Participant>("newPass", (data) =>
                {
                    foreach (ParticipantClass c in data.Classes)
                    {
                        myHub.Invoke<ICollection<Participant>>("GetCurrentResults", c.Id)
                            .ContinueWith((result) => 
                            {
                                var d = result.Result;
                                var json = JsonConvert.SerializeObject(d, Formatting.Indented);
                                System.IO.File.WriteAllText(@"c:\temp\" + c.Name + ".json", json);
                            });
                    }
                }
            );
            Console.ReadLine();

            myHubConn.Stop();
            Console.WriteLine("Stopped");
        }
    }
}
