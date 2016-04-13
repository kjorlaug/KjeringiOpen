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
using System.Threading;

using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Tables;

namespace PrintConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            String hub = ConfigurationManager.AppSettings["hub"];
            String name = ConfigurationManager.AppSettings["name"];

            Participant p = new Participant() {
                Name = "Test testesen",
                Startnumber = 1,
                EmitID = 1,
                Classes = new List<ParticipantClass>() {new ParticipantClass() {Name = "Test", Id= "test"}},
                TeamMembers = new List<string>()
            };
            p.Positions = new Dictionary<int,Dictionary<string,int>>();
            p.Positions.Add(248, new Dictionary<string,int>());
            p.Positions[248].Add("test", 1);
            //p.Splits = new List<Result>();

            PrintParticiant(p);
            return;

            HubConnection myHubConn;
            IHubProxy myHub;
           
            myHubConn = new HubConnection(hub);
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHubConn.Start()
                .ContinueWith((prevTask) =>
                {
                    myHub.Invoke("Join", "Printer - " + name);
                    myHub.Invoke("AddtoGroup", "EmitReader");
                    myHub.Invoke("AddtoGroup", "248");
                }).Wait();

            myHub.On<Participant>("newPass", (data) =>
            {
                PrintParticiant(data);
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

        private static void PrintParticiant(Participant data)
        {

            //Create a pdf document.<br>
            PdfDocument doc = new PdfDocument();
            
            //margin
            PdfUnitConvertor unitCvtr = new PdfUnitConvertor();
            PdfMargins margin = new PdfMargins();
            margin.Top = unitCvtr.ConvertUnits(2.54f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Bottom = margin.Top;
            margin.Left = unitCvtr.ConvertUnits(3.17f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Right = margin.Left;
            
            // Create new page
            PdfPageBase page = doc.Pages.Add(PdfPageSize.A4, margin);
            float y = 10;
            
            //title
            PdfBrush brush1 = PdfBrushes.Black;
            PdfTrueTypeFont font1 = new PdfTrueTypeFont(new Font("Arial", 16f, FontStyle.Bold));
            PdfStringFormat format1 = new PdfStringFormat(PdfTextAlignment.Center);

            //StringBuilder sb = new StringBuilder(HtmlTemplate.Template.Replace("#startnummer#", data.Startnumber.ToString()).Replace("#brikke#", data.EmitID.ToString()));

            page.Canvas.DrawString(data.Name, font1, brush1, page.Canvas.ClientSize.Width / 2, y, format1);
            y = y + font1.MeasureString(data.Name, format1).Height + 15;

            StringBuilder sb = new StringBuilder(String.Join(", ", data.Classes.Select(p => p.Name).ToList<String>()));
            sb.Append(" - ");
            sb.Append(String.Join(", ", data.Positions[248].Select(p => p.Value.ToString()).ToList<String>()));
            sb.Append(".plass");

            page.Canvas.DrawString(sb.ToString(), font1, brush1, page.Canvas.ClientSize.Width / 2, y, format1);            
            y += font1.MeasureString(sb.ToString(), format1).Height + 15;

            page.Canvas.DrawString("Etappetider", font1, brush1, page.Canvas.ClientSize.Width / 2, y, format1);            
            y += font1.MeasureString("Etappetider", format1).Height + 15;
            
            String[][] tableDataSource = new String[][] {
                new String[] {"Telemark", (data.TeamMembers.Count == 4)? data.TeamMembers[0] : "", (data.Splits.Count == 4) ? data.Splits[0].Time : ""},
                new String[] {"Ski", (data.TeamMembers.Count == 4)? data.TeamMembers[1] : "", (data.Splits.Count == 4) ? data.Splits[1].Time : ""},
                new String[] {"Springing", (data.TeamMembers.Count == 4)? data.TeamMembers[2] : "", (data.Splits.Count == 4) ? data.Splits[2].Time : ""},
                new String[] {"Sykling", (data.TeamMembers.Count == 4)? data.TeamMembers[3] : "", (data.Splits.Count == 4) ? data.Splits[3].Time : ""},
                new String[] {"Totaltid", "", data.TotalTime}
            };

            PdfTable table = new PdfTable();
            table.Style.CellPadding = 2;
            table.Style.HeaderSource = PdfHeaderSource.Rows;
            table.Style.HeaderRowCount = 1;
            table.Style.ShowHeader = true;
            table.DataSource = tableDataSource;

            PdfLayoutResult result = table.Draw(page, new PointF(0, y));
            
            y = y + result.Bounds.Height + 5;
            
            PdfBrush brush2 = PdfBrushes.Gray;
            PdfTrueTypeFont font2 = new PdfTrueTypeFont(new Font("Arial", 9f));

            doc.SaveToFile(@"c:\temp\" + data.EmitID + ".pdf");
            
            doc.PrintDocument.Print();

            //String file = @"c:\temp\" + data.EmitID + ".html";

            //System.IO.StreamWriter sw = new System.IO.StreamWriter(file, false, System.Text.UTF8Encoding.UTF8);
            //sw.Write(sb.ToString());
            //sw.Close();
        }
    }
}
