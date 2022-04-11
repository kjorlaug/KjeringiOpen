using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using KjeringiData;
using Newtonsoft.Json;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using EmitReaderLib.Model;
using Spire.Pdf.Tables;
using System.Text;
using System.IO;
using System.Drawing;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Results");
        }

        public ActionResult Status()
        {
            //return RedirectToAction("Results");
            return View();
        }

        public ActionResult EmitReader()
        {
            return View();
        }

        public ActionResult Participants()
        {
            //ViewBag.Data = TheRace.Instance.Participants.OrderBy(p => p.Startnumber).ToList<Participant>();
            //ViewBag.Year = int.Parse(TheRace.Instance.Name);

            TempData["participants"] = TheRace.Instance.Participants.OrderBy(p => p.Startnumber).ToList<Participant>();
            return View();
        }

        public ActionResult Results(int? id)
        {
            if (id.HasValue)
                ViewBag.Race = TheRace.Historical(id.Value);
            else
                ViewBag.Race = TheRace.Instance;

            return View();
        }

        public ActionResult DataSupers(int? id)
        {
            EmitReaderLib.Race r = id.HasValue ? TheRace.Historical(id.Value) : TheRace.Instance;

            return Json(
                r.Participants.Where(p => p.IsSuper && !p.Classes.Exists(c => c.Id.Equals("TEST")))
                .Select(p => new
                {
                    id = p.Id,
                    classCode = p.Classes[0].Id,
                    norwayCup = p.Classes.Exists(c => c.Name.StartsWith("NM")) ? 1 : 0,
                    chipNumber = p.EmitID,
                    startNumber = p.Startnumber,
                    companyName = p.CompanyName,
                    notASuper = 0,
                    firstname = p.Name.Split(' ')[0],
                    surname = p.Name.Substring(p.Name.IndexOf(" ")).TrimStart(),
                    birthYear = int.Parse(r.Name) - p.Ages[0],
                    gender = p.Gender[0] == 1 ? "G" : "J",
                    //tshirtCode = p.ShirtSizes[0],
                    phone = p.Telephone[0],
                    club = p.Club
                }).ToList(), JsonRequestBehavior.AllowGet);
            
        }

        public ActionResult DataTeams(int? id)
        {
            EmitReaderLib.Race r = id.HasValue ? TheRace.Historical(id.Value) : TheRace.Instance;
            /*         
             "name": "1gongi\u00e5ret",
            "club": "",
            "id": "24",
            "classCode": "LOV",
            "companyName": "",
            "chipNumber": "3378",
            "startNumber": "31",
*/
            return Json(
                r.Participants.Where(p => p.IsTeam && !p.Classes.Exists(c => c.Id.Equals("TEST")))
                .Select(p => new
                {
                    name = p.Name,
                    id = p.Id,
                    classCode = p.Classes[0].Id,
                    chipNumber = p.EmitID,
                    startNumber = p.Startnumber,
                    companyName = p.CompanyName,
                    club = p.Club,
                    members = new[]
                    {
                        new {
                            id = p.Id,
                            classCode = p.Classes[0].Id,
                            norwayCup = p.Classes.Exists(c => c.Name.StartsWith("NM")) ? 1 : 0,
                            chipNumber = p.EmitID,
                            startNumber = p.Startnumber,
                            companyName = p.CompanyName,
                            notASuper = p.Startnumber > 99 ? 0 : 1,
                            firstname = p.TeamMembers[0].Split(' ')[0],
                            surname = p.TeamMembers[0].Substring(p.TeamMembers[0].IndexOf(" ")).TrimStart(),
                            birthYear = int.Parse(r.Name) - p.Ages[0],
                            gender = p.Gender[0] == 1 ? "G" : "J",
                            phone = p.Telephone.Count > 0 ? p.Telephone[0] : "",
                            club = p.Club
                        }, 
                        new {
                            id = p.Id,
                            classCode = p.Classes[0].Id,
                            norwayCup = p.Classes.Exists(c => c.Name.StartsWith("NM")) ? 1 : 0,
                            chipNumber = p.EmitID,
                            startNumber = p.Startnumber,
                            companyName = p.CompanyName,
                            notASuper = 1,
                            firstname = p.TeamMembers[1].Split(' ')[0],
                            surname = p.TeamMembers[1].Substring(p.TeamMembers[1].IndexOf(" ")).TrimStart(),
                            birthYear = int.Parse(r.Name) - p.Ages[1],
                            gender = p.Gender[1] == 1 ? "G" : "J",
                            phone = p.Telephone.Count > 1 ? p.Telephone[1] : "",
                            club = p.Club
                        },
                        new {
                            id = p.Id,
                            classCode = p.Classes[0].Id,
                            norwayCup = p.Classes.Exists(c => c.Name.StartsWith("NM")) ? 1 : 0,
                            chipNumber = p.EmitID,
                            startNumber = p.Startnumber,
                            companyName = p.CompanyName,
                            notASuper = 1,
                            firstname = p.TeamMembers[2].Split(' ')[0],
                            surname = p.TeamMembers[2].Substring(p.TeamMembers[2].IndexOf(" ")).TrimStart(),
                            birthYear = int.Parse(r.Name) - p.Ages[2],
                            gender = p.Gender[2] == 1 ? "G" : "J",
                            phone = p.Telephone.Count > 2 ? p.Telephone[2] : "",
                            club = p.Club
                        },
                        new {
                            id = p.Id,
                            classCode = p.Classes[0].Id,
                            norwayCup = p.Classes.Exists(c => c.Name.StartsWith("NM")) ? 1 : 0,
                            chipNumber = p.EmitID,
                            startNumber = p.Startnumber,
                            companyName = p.CompanyName,
                            notASuper = 1,
                            firstname = p.TeamMembers[3].Split(' ')[0],
                            surname = p.TeamMembers[3].Substring(p.TeamMembers[3].IndexOf(" ")).TrimStart(),
                            birthYear = int.Parse(r.Name) - p.Ages[3],
                            gender = p.Gender[3] == 1 ? "G" : "J",
                            phone = p.Telephone.Count > 3 ? p.Telephone[3] : "",
                            club = p.Club
                        }
                    }
                }).ToList(), JsonRequestBehavior.AllowGet);

        }

        public ActionResult ResultsNM(int? id)
        {
            if (id.HasValue)
                ViewBag.Race = TheRace.Historical(id.Value);
            else
                ViewBag.Race = TheRace.Instance;

            return View();
        }
        public ActionResult TopSplits(int? id, int? name, int? ageclass, int? gender)
        {
            if (id.HasValue)
                ViewBag.Race = TheRace.Historical(id.Value);
            else
                ViewBag.Race = TheRace.Instance;

            ViewBag.StationId = name ?? 90;
            ViewBag.ageclass = ageclass ?? 0;
            ViewBag.gender = gender ?? 0;

            return View();
        }

        public ActionResult Live(int? name)
        {
            ViewBag.StationFinishId = TheRace.Instance.TimeStations.Where(ts => ts.Finish).First().Id.ToString();
            ViewBag.StationIncomingId = TheRace.Instance.TimeStations.Where(ts => ts.Official).OrderByDescending(ts => ts.Sequence).Skip(1).First().Id.ToString();
            ViewBag.Race = TheRace.Instance;

            if (name.HasValue)
                ViewBag.StationId = name.ToString();
            else
                ViewBag.StationId = ViewBag.StationFinishId;

            ViewBag.StationName = TheRace.Instance.TimeStations.Find(ts => ts.Id.ToString().Equals(ViewBag.StationId)).Name;

            return View();
        }

        public ActionResult Speaker()
        {
            ViewBag.Race = TheRace.Instance;
            return View();
        }

        public FileResult Download(int year, int id)
        {
            //Create a pdf document.<br>
            PdfDocument doc = new PdfDocument();
            Participant data = TheRace.Historical(year).Participants.Where(p => p.Startnumber == id).First();

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

            // Brand year
            page.Canvas.DrawString("Kjeringi Open " + year.ToString(), font20b, brush1, 60, y + 60, format1);

            //Draw the image
            PdfImage image = PdfImage.FromFile(Server.MapPath("~/Content/Images/price.jpg"));
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

            List<String[]> splits = data.Splits(data.Classes[0].Id).Select(p => new String[] { p.IsSuper ? "" : p.Name, p.Leg, (p.Estimated ? "(mangler)" : p.Time) }).ToList<String[]>();

            if (splits.Count() > 0)
            {               
                splits.Add(new String[] { "", "Totaltid", data.TotalTime });

                // Are all names in the splits are empty, remove
                bool allEmpty = true;
                foreach (String[] arr in splits)
                    allEmpty = allEmpty && String.IsNullOrEmpty(arr[0]);

                if (allEmpty)
                {
                    for (int i = 0; i < splits.Count; i++)
                    {
                        List<String> arr = splits[i].ToList<String>();
                        arr.RemoveAt(0);
                        splits[i] = arr.ToArray<String>();    
                    }
                }

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

                table.DataSourceType = PdfTableDataSourceType.TableDirect;
                table.DataSource = splits.ToArray<String[]>();

                table.Columns[table.Columns.Count - 2].StringFormat = new PdfStringFormat(PdfTextAlignment.Right);
                table.Columns[table.Columns.Count - 2].Width = 4.0f;

                table.Columns[table.Columns.Count - 1].StringFormat = new PdfStringFormat(PdfTextAlignment.Right);
                table.Columns[table.Columns.Count - 1].Width = 4.0f;

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

            image = PdfImage.FromFile(Server.MapPath("~/Content/Images/kjeringi2016_logo-2.png"));
            width = image.Width;
            height = image.Height;
            x = page.Canvas.ClientSize.Width - width;
            page.Canvas.DrawImage(image, 0, y, width, height);

            image = PdfImage.FromFile(Server.MapPath("~/Content/Images/difi-logo.png"));
            width = image.Width;
            height = image.Height;
            x = page.Canvas.ClientSize.Width - width;
            page.Canvas.DrawImage(image, x, y, width, height);

            System.IO.MemoryStream file = new System.IO.MemoryStream();
            doc.SaveToStream(file);
            return File(file.ToArray(), "application/pdf");
        }
    }
}