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

            myHubConn.Start()
                .ContinueWith((prevTask) =>
                {
                    myHub.Invoke("Join", "ResultService");
                    myHub.Invoke("AddtoGroup", "");
                }).Wait();

            myHub.On<Result>("processResultat", (data) =>
                {
                    foreach (ParticipantClass c in data.ParticipantClasses)
                    {
                        var results = myHub.Invoke<IEnumerable<Result>>("GetCurrentResults", c.Id, data.TimeStation.Id);
                        var json = JsonConvert.SerializeObject(results);

                        System.IO.File.WriteAllText(@"c:\temp\" + c.Name + ".json", json);
                    }
                }
            );
            Console.ReadLine();

            myHubConn.Stop();
            Console.WriteLine("Stopped");
        }
    }
}
