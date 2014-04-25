using System;
using System.Configuration;
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

        static Queue<String> printQueue = new Queue<string>();

        static void Main(string[] args)
        {
            String hub = ConfigurationManager.AppSettings["hub"];
            String name = ConfigurationManager.AppSettings["name"];

            HubConnection myHubConn;
            IHubProxy myHub;
           
            myHubConn = new HubConnection(hub);
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHubConn.Start()
                .ContinueWith((prevTask) =>
                {
                    myHub.Invoke("Join", "Printer - " + name);
                    myHub.Invoke("AddtoGroup", "EmitReader");
                    myHub.Invoke("AddtoGroup", "Mål");
                }).Wait();

            myHub.On<KjeringiData.ResultatData>("processResultat", (data) =>
            {
                StringBuilder sb = new StringBuilder(HtmlTemplate.Template.Replace("#startnummer#", data.Startnummer).Replace("#brikke#", data.EmitID));

                sb.Append(data.Namn);
                sb.Append("<br><br>");

                sb.Append(data.Klasse);
                sb.Append(" - ");
                sb.Append(data.PlasseringIKlasse);
                sb.Append(".plass <br><br><h3>Etappetider</h3><table id='customers'><tr><th>Veksling</th><th>Ut&oslash;var</th><th>Etappetid</th></tr>");
                
                sb.Append("<tr><td>Telemark</td><td>");
                if (data.Medlemmer.Count == 4)
                    sb.Append(data.Medlemmer[0]);
                sb.Append("</td><td>");
                if (data.Etappetider.Count == 4)
                    sb.Append(data.Etappetider[0]);
                sb.Append("</td></tr>");

                sb.Append("<tr class='alt'><td>Ski</td><td>");
                if (data.Medlemmer.Count == 4)
                    sb.Append(data.Medlemmer[1]);
                sb.Append("</td><td>");
                if (data.Etappetider.Count == 4)
                    sb.Append(data.Etappetider[1]);
                sb.Append("</td></tr>");

                sb.Append("<tr><td>Springing</td><td>");
                if (data.Medlemmer.Count == 4 )
                    sb.Append(data.Medlemmer[2]);
                sb.Append("</td><td>");
                if (data.Etappetider.Count == 4)
                    sb.Append(data.Etappetider[2]);
                sb.Append("</td></tr>");

                sb.Append("<tr class='alt'><td>Sykling</td><td>");
                if (data.Medlemmer.Count == 4)
                    sb.Append(data.Medlemmer[3]);
                sb.Append("</td><td>");
                if (data.Etappetider.Count == 4)
                    sb.Append(data.Etappetider[3]);
                sb.Append("</td></tr>");

                sb.Append("<tr><th>Totaltid</td><td>");
                sb.Append("</th><th>");
                sb.Append(data.TotalTid);
                sb.Append("</th></th>");

                sb.Append("</table></div><footer></footer></body></html>");

                String file = @"c:\temp\" + data.EmitID + ".html";

                System.IO.StreamWriter sw = new System.IO.StreamWriter(file, false, System.Text.UTF8Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();

                PrinterSettings ps = new PrinterSettings();
                RawPrinterHelper.SendFileToPrinter(ps.PrinterName, file);

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
