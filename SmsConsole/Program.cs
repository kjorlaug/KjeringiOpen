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

namespace SmsConsole
{
    class Program {

        static void Main(string[] args)
        {
            String hub = ConfigurationManager.AppSettings["hub"];
            String name = ConfigurationManager.AppSettings["name"];
            String smsPass = ConfigurationManager.AppSettings["pass"];

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

            myHub.On<KjeringiData.ResultatData>("processResultat", (data) =>
                {                    
                    // In production?
                    if ((data.Test || DateTime.Now.Hour > 12) && ((data.Test && !data.Offisiell) || data.Offisiell))
                    {
                        if (!String.IsNullOrEmpty(data.Telefonnummer))
                        {
                            StringBuilder sb = new StringBuilder();

                            if (data.Test)
                                sb.Append("Test: ").Append(data.EmitID).Append(" ");

                            String pass = data.ResultatForEtappe;
                            if (pass.Equals("Mål"))
                                pass = "Sykkel";
                            else if (pass.Equals("Øvstestølen"))
                                pass = "Ski";
                            else if (pass.Equals("Damefall"))
                                pass = "Telemark";
                            else if (pass.Equals("Barneskulen"))
                                pass = "Springing";

                            sb.Append("Etappe: ").Append(pass);

                            List<String> et = data.Etappetider.FindAll(x => !String.IsNullOrEmpty(x));
                            
                            if (et.Count > 0)
                                sb.Append(" Tid:").Append(et.Last());
                            if (et.Count > 1)
                                sb.Append(" Total: ").Append(data.TotalTid);
                            sb.Append(" Plass: ").Append(data.PlasseringIKlasse);
                            sb.Append(" SMS-tjenestene levert av PSWinCom");

                            foreach (String tlf in data.Telefonnummer.Split(','))
                            {
                                try
                                {
                                    String url = String.Format(@"http://sms.pswin.com/http4sms/send.asp?USER=kjeringiopen&{0}&RCV=47{1}&TXT={2}&snd=Kjeringi", smsPass, tlf,  sb.ToString());
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
                    }
                    else if (!String.IsNullOrEmpty(data.Telefonnummer))
                        Console.WriteLine("Not in prod - should sms " + data.Telefonnummer);
                }
            );
            Console.ReadLine();

            myHubConn.Stop();
            Console.WriteLine("Stopped");

        }
    }
}
