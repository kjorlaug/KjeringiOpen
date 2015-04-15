using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Printing;
using Microsoft.AspNet.SignalR.Client;

using EmitReaderLib;
using EmitReaderLib.Model;

namespace PrintConsole
{
    class Program
    {
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

            //myHub.On<Result>("processResultat", (data) =>
            //{
            //    StringBuilder sb = new StringBuilder(HtmlTemplate.Template.Replace("#startnummer#", data.Startnumber.ToString()).Replace("#brikke#", data.EmitID.ToString()));

            //    sb.Append(data.Name);
            //    sb.Append("<br><br>");

            //    sb.Append(String.Join(", ", data.ParticipantClasses));
            //    sb.Append(" - ");
            //    sb.Append(String.Join(", ", data.Positions.Values.ToArray()));
            //    sb.Append(".plass <br><br><h3>Etappetider</h3><table id='customers'><tr><th>Veksling</th><th>Ut&oslash;var</th><th>Etappetid</th></tr>");
                
            //    sb.Append("<tr><td>Telemark</td><td>");
            //    if (data.TeamMembers.Count == 4)
            //        sb.Append(data.TeamMembers[0]);
            //    sb.Append("</td><td>");
            //    if (data.Splits.Count == 4)
            //        sb.Append(data.Splits[0]);
            //    sb.Append("</td></tr>");

            //    sb.Append("<tr class='alt'><td>Ski</td><td>");
            //    if (data.TeamMembers.Count == 4)
            //        sb.Append(data.TeamMembers[1]);
            //    sb.Append("</td><td>");
            //    if (data.Splits.Count == 4)
            //        sb.Append(data.Splits[1]);
            //    sb.Append("</td></tr>");

            //    sb.Append("<tr><td>Springing</td><td>");
            //    if (data.TeamMembers.Count == 4 )
            //        sb.Append(data.TeamMembers[2]);
            //    sb.Append("</td><td>");
            //    if (data.Splits.Count == 4)
            //        sb.Append(data.Splits[2]);
            //    sb.Append("</td></tr>");

            //    sb.Append("<tr class='alt'><td>Sykling</td><td>");
            //    if (data.TeamMembers.Count == 4)
            //        sb.Append(data.TeamMembers[3]);
            //    sb.Append("</td><td>");
            //    if (data.Splits.Count == 4)
            //        sb.Append(data.Splits[3]);
            //    sb.Append("</td></tr>");

            //    sb.Append("<tr><th>Totaltid</td><td>");
            //    sb.Append("</th><th>");
            //    sb.Append(data.Total);
            //    sb.Append("</th></th>");

            //    sb.Append("</table></div><footer></footer></body></html>");

            //    String file = @"c:\temp\" + data.EmitID + ".html";

            //    System.IO.StreamWriter sw = new System.IO.StreamWriter(file, false, System.Text.UTF8Encoding.UTF8);
            //    sw.Write(sb.ToString());
            //    sw.Close();

            //    PrinterSettings ps = new PrinterSettings();
            //    RawPrinterHelper.SendFileToPrinter(ps.PrinterName, file);

            //});

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
