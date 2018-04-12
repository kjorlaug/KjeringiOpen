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
using Newtonsoft.Json;

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
                    myHub.Invoke("AddtoGroup", "0");
                    myHub.Invoke("AddtoGroup", "248");
                }).Wait();

            myHub.On<Participant>("newPass", (data) =>
            {
                try
                {
                    Console.WriteLine("Print " + data.Name);
                    PrintParticiant(data);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
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
            margin.Top = unitCvtr.ConvertUnits(1f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Bottom = margin.Top;
            margin.Left = unitCvtr.ConvertUnits(1f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Right = margin.Left;

            // Create new page
            PdfPageBase page = doc.Pages.Add(PdfPageSize.A4, margin);
            float y = 10;

            //title
            PdfBrush brush1 = PdfBrushes.Black;

            PdfFont font32b = new PdfFont(PdfFontFamily.Helvetica, 40f, PdfFontStyle.Bold); ;
            PdfFont font32 = new PdfFont(PdfFontFamily.TimesRoman, 32f, PdfFontStyle.Regular);
            PdfFont font20b = new PdfFont(PdfFontFamily.TimesRoman, 20f, PdfFontStyle.Bold);
            PdfFont font20 = new PdfFont(PdfFontFamily.TimesRoman, 18f, PdfFontStyle.Regular);
            PdfStringFormat format1 = new PdfStringFormat(PdfTextAlignment.Left);
            format1.WordWrap = PdfWordWrapType.Word;


            //Draw the image
            PdfImage image = PdfImage.FromFile(@"price.jpg");
            float width = image.Width;
            float height = image.Height;
            float x = page.Canvas.ClientSize.Width - width;
            page.Canvas.DrawImage(image, x, y, width, height);

            page.Canvas.DrawString(data.Splits(data.Classes[0].Id).Last().Position.ToString() + ".", font32b, brush1, x + 60, y + 50, format1);

            y = image.Height;

            page.Canvas.DrawString(data.Name, font32b, brush1, 0, y, format1);

            y = y + font32b.MeasureString(data.Name, format1).Height + 15;

            foreach (ParticipantClass c in data.Classes.Where(c => c.Official))
            {
                StringBuilder sb = new StringBuilder(c.Name);
                sb.Append(" - ");
                var res = data.Leg(c.Id, 248);
                if (res != null)
                    sb.Append(data.Leg(c.Id, 248).Position);
                sb.Append(".plass");

                page.Canvas.DrawString(sb.ToString(), font20, brush1, 0, y, format1);
                y += font20.MeasureString(sb.ToString(), format1).Height + 15;
            }

            List<String[]> splits = data.Splits(data.Classes[0].Id).Select(p => new String[] { p.Leg, p.IsSuper ? "" : p.Name, (p.Estimated ? "(mangler)" : p.Time) }).ToList<String[]>();
            if (splits.Count() > 0)
            {
                splits.Add(new String[] { "Totaltid", "", data.TotalTime });

                y = y + 30;

                page.Canvas.DrawString("Etappetider", font20b, brush1, 0, y, format1);
                y += font20b.MeasureString("Etappetider", format1).Height + 5;

                PdfTable table = new PdfTable();
                table.Style.BorderPen = new PdfPen(Color.Transparent);
                table.Style.DefaultStyle.TextBrush = brush1;
                table.Style.DefaultStyle.Font = font20;
                table.Style.DefaultStyle.BorderPen = new PdfPen(Color.Transparent);
                table.Style.CellPadding = 2;
                table.Style.HeaderSource = PdfHeaderSource.Rows;
                table.Style.HeaderRowCount = 0;
                table.Style.ShowHeader = false;
                table.Style.AlternateStyle = new PdfCellStyle();

                table.Style.AlternateStyle.TextBrush = brush1;
                table.Style.AlternateStyle.Font = font20;
                table.Style.AlternateStyle.BackgroundBrush = PdfBrushes.LightGray;
                table.Style.AlternateStyle.BorderPen = new PdfPen(Color.Transparent);

                table.DataSource = splits.ToArray<String[]>();

                PdfLayoutResult result = table.Draw(page, new PointF(0, y));

                y = y + result.Bounds.Height + 5;

            }

            StringBuilder s = new StringBuilder("Start # ");
            s.Append(data.Startnumber.ToString());
            s.Append("    Emit # ");
            s.Append(data.EmitID.ToString());

            y += 50;

            page.Canvas.DrawString(s.ToString(), font20, brush1, page.Canvas.ClientSize.Width / 2, y, format1);

            y = page.Canvas.ClientSize.Height - 60;

            image = PdfImage.FromFile(@"kjeringi2016_logo-2.png");
            width = image.Width;
            height = image.Height;
            x = page.Canvas.ClientSize.Width - width;
            page.Canvas.DrawImage(image, 0, y, width, height);

            image = PdfImage.FromFile(@"difi-logo.png");
            width = image.Width;
            height = image.Height;
            x = page.Canvas.ClientSize.Width - width;
            page.Canvas.DrawImage(image, x, y, width, height);
            doc.PrintDocument.Print();

            doc.SaveToFile(@"c:\temp\" + data.EmitID + ".pdf");
        }
    }
}
