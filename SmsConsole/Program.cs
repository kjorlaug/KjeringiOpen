using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNet.SignalR.Client;

using EmitReaderLib;
using EmitReaderLib.Model;

namespace SmsConsole
{
    class Program {

        static void Main(string[] args)
        {
            String hub = ConfigurationManager.AppSettings["hub"];
            String name = ConfigurationManager.AppSettings["name"];
            String smsPass = ConfigurationManager.AppSettings["pass"];

            Boolean copy = true;

            HubConnection myHubConn;
            IHubProxy myHub;

            myHubConn = new HubConnection(hub);
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHubConn.Start()
                .ContinueWith((prevTask) =>
                {
                    myHub.Invoke("Join", "SMS - " + name);
                    myHub.Invoke("AddtoGroup", "");
                }).Wait();

            myHub.On<Participant>("newPass", (data) =>
                {
                    // In production?
                    StringBuilder sb = new StringBuilder();

                    sb.Append("Mellombels resultat Kjeringi Open 2018 %0A");
                    sb.Append(data.Name);
                    sb.Append(" (");
                    sb.Append(data.Classes[0].Name);
                    sb.Append(")%0AEtapper:%0A");

                    List<String[]> splits = data.Splits(data.Classes[0].Id).Select(p => new String[] { p.Leg, p.IsSuper ? "" : p.Name, (p.Estimated ? "(mangler)" : p.Time) }).ToList<String[]>();

                    foreach (Result r in data.Splits(data.Classes[0].Id))
                    {
                        sb.Append(" ");
                        sb.Append(r.Leg);
                        sb.Append(" ");
                        sb.Append(r.Estimated ? "(mangler)" : r.Time);
                        sb.Append(" (");
                        sb.Append(r.Position);
                        sb.Append(".plass) ");
                        sb.Append("%0A");
                    }
                    sb.Append("Totaltid: ");
                    sb.Append(data.TotalTime);

                    sb.Append("%0ASMS-tjenestene levert av Difi i samarbeid med Linkmobility");

                    foreach (String tlf in data.Telephone.Distinct())
                    {
                        try
                        {
                            String url = String.Format(@"http://simple.pswin.com/?USER=kjeringiopen&{0}&RCV=47{1}&TXT={2}&snd=Kjeringi&ENC=utf-8", "PW=0DgFPq2k3", tlf, sb.ToString());
                            WebClient webClient = new WebClient();
                            Stream stream = webClient.OpenRead(url);
                            StreamReader reader = new StreamReader(stream);
                            String request = reader.ReadToEnd();
                            Console.WriteLine("Success: " + url);
                        }
                        catch (WebException ex)
                        {
                        }
                    }
                }
            );
            Console.ReadLine();

            myHubConn.Stop();
            Console.WriteLine("Stopped");
        }
    }
}
