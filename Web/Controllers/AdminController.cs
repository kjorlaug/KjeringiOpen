using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using EmitReaderLib.Model;
using KjeringiData;

namespace Web.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            TempData["testmode"] = TheRace.Instance.InTestMode;
            return View();
        }

        public ActionResult SetTestMode()
        {
            TheRace.Instance.InTestMode = !TheRace.Instance.InTestMode;
            TempData["LogMessage"] = "Testmodus " + (TheRace.Instance.InTestMode ? "på" : "av");
            return RedirectToAction("Index");
        }

        public ActionResult Reset()
        {
            TheRace.Reset();
            TempData["LogMessage"] = "Reset utført!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AssignNumbers(String cardidseries)
        {
            if (String.IsNullOrWhiteSpace(cardidseries))
            {
                TempData["ErrorMessage"] = "Oppgje brikkeserier!";
                return RedirectToAction("index");
            }

            // Validate input
            List<String> series = new List<String>();
            series.AddRange(cardidseries.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None));

            List<KeyValuePair<int, int>> numbers = new List<KeyValuePair<int, int>>();
            Regex tester = new Regex("^[0-9]+(-[0-9]+)");
            foreach (String s in series)
            {
                if (tester.IsMatch(s))
                    numbers.Add(new KeyValuePair<int, int>(int.Parse(s.Substring(0, s.IndexOf('-'))), int.Parse(s.Substring(s.IndexOf('-') + 1))));
                else
                {
                    TempData["ErrorMessage"] = "Ugyldig serie: " + s;
                    return RedirectToAction("index");
                }
            }

            MySqlConnection conn = new MySqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["Kjeringi"].ConnectionString);
            conn.Open();

            Boolean first = true;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("select card from (");
            foreach (KeyValuePair<int, int> p in numbers)
                for (int x = p.Key; x <= p.Value; x++)
                {
                    if (!first)
                    {
                        sb.Append(" UNION SELECT ");
                        sb.Append(x);
                    }
                    else
                    {
                        first = false;
                        sb.Append("SELECT ");
                        sb.Append(x);
                        sb.Append(" as card ");
                    }
                }
            sb.Append(") c WHERE c.card not in (SELECT ifnull(chipNumber, 0) FROM kop_person UNION all SELECT ifnull(chipNumber, 0) FROM kop_team) ");

            // reset all
            var cmd = new MySqlCommand(@"update kop_team set startNumber = null, chipNumber = null", conn);
            cmd.ExecuteNonQuery();

            cmd = new MySqlCommand(@"update kop_person set startNumber = null, chipNumber = null", conn);
            cmd.ExecuteNonQuery();

            // Lag utan super
            cmd = new MySqlCommand(@"
                update kop_team target inner join (
                select 
	                t.id, t.name, t.startNumber, t.chipNumber, @row := @row + 1 as start
                from 
	                kop_team t 
	                inner join kop_person p, 
	                (select @row := 30) r
                where 
	                t.id = p.teamId 
                and p.sprintNumber = 1 
                and p.superWife = 0 
                and t.deleted = 0
                order by t.teamClassCode, t.id
                ) source on target.id = source.id
                set
	                target.startNumber = start,
	                target.chipNumber = (SELECT card FROM (SELECT card, (@s := @s + 1) as sequence FROM (" + sb.ToString() + ") cardsNotInUse, (select @s := 1) seq) availableCards WHERE availableCards.sequence = (start - 29))", conn);

            var numberTeamNoSuper = cmd.ExecuteNonQuery();

            // Lag med super
            cmd = new MySqlCommand(@"
                update kop_team target inner join (
                select 
	                t.id, t.name, t.startNumber, t.chipNumber, @row := @row + 1 as start
                from 
	                kop_team t 
	                inner join kop_person p, 
	                (select @row :=100) r	
                where 
	                t.id = p.teamId 
                and p.sprintNumber = 1 
                and p.superWife = 1 
                and t.deleted = 0
                order by t.teamClassCode, t.id
                ) source on target.id = source.id
                set
	                target.startNumber = start,
	                target.chipNumber = (SELECT card FROM (SELECT card, (@s := @s + 1) as sequence FROM (" + sb.ToString() + ") cardsNotInUse, (select @s := 1) seq) availableCards WHERE availableCards.sequence = (start - 99))", conn);

            var numberTeamSuper = cmd.ExecuteNonQuery();

            // Super utan lag
            cmd = new MySqlCommand(@"
                update kop_person p, (select @row :=1000) r
                set p.startNumber = case when @row >= 1100 then (@row := @row + 1) + 49 else @row := @row + 1 end,
                chipNumber = (SELECT card FROM (SELECT card, (@s := @s + 1) as sequence FROM (" + sb.ToString() + @") cardsNotInUse, (select @s := 1) seq) availableCards WHERE availableCards.sequence = (@row - 999))
                where isnull(p.teamId) and p.superWife = 1 and p.deleted = 0", conn);

            var numberSuper = cmd.ExecuteNonQuery();

            // Super med lag
            cmd = new MySqlCommand(@"
                update kop_person p inner join kop_team t on p.teamId = t.id
                set 
                    p.startNumber = t.startNumber + 1000,
                    p.chipNumber = (SELECT card FROM (SELECT card, (@s := @s + 1) as sequence FROM (" + sb.ToString() + @") cardsNotInUse, (select @s := 1) seq) availableCards WHERE availableCards.sequence = (t.startNumber - 99))
                where p.superWife = 1 and p.deleted = 0", conn);

            var numberSuperWithTeam = cmd.ExecuteNonQuery();

            TempData["LogMessage"] = String.Format("Antall lag: {0}, lag med super: {1} og super {2}", numberTeamNoSuper, numberTeamSuper, numberSuper);

            KjeringiData.TheRace.Reset();

            return RedirectToAction("index");

        }

        public ActionResult CreateSuperStickers()
        {
            TempData["supers"] = TheRace.Instance.Participants.Where(p => p.IsSuper).ToList<Participant>();
            return View();
        }
        public ActionResult CreatePackageStickers()
        {
            TempData["participants"] = TheRace.Instance.Participants.OrderBy(p => p.Startnumber).ToList<Participant>();
            return View();
        }
    }
}