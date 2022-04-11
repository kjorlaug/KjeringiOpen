using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using MySql.Data.MySqlClient;

namespace EmitReaderLib.Builders
{
    public class MySqlRaceBuilder2014 : IRaceBuilder
    {
        public MySqlRaceBuilder2014(String connection, String year)
        {
            _conn = connection;
            _year = year;
        }

        protected String _conn { get; set; }
        protected String _year{ get; set; }

        public void BuildRace(Race race)
        {
            race.Name = _year;
            _year = "kop";

            MySqlConnection conn = new MySqlConnection(_conn);
            conn.Open();

            // Adding supers
            var cmd = new MySqlCommand(@"select startNumber, chipNumber as cardid, firstname, surname, personClassCode, ifnull(companyClass, 0) as companyClass, phoneNumber, gender, birthyear, club, companyName from " + _year + "_person where superwife = 1 and deleted = 0 and startnumber is not null and chipNumber is not null", conn);
            var data = cmd.ExecuteReader();

            while (data.Read())
            {
                var p = new Participant();

                p.Startnumber = data.GetInt32("startNumber");
                p.EmitID = int.Parse(data.GetString("cardid"));
                p.Name = data.GetString("firstname") + " " + data.GetString("surname");
                p.Telephone = new List<String>() { data.GetString("phoneNumber") };
                p.Classes = new List<ParticipantClass>() { race.Classes.Find(x => x.Id.Equals(data.GetString("personClassCode"))) };
                p.IsTeam = false;
                p.IsSuper = true;
                p.Gender = new List<int>() { data.GetString("gender").Equals("G") ? 1 : 2 };
                p.Ages = new List<int> { int.Parse(race.Name) - data.GetInt32("birthYear") };

                p.IsCompany = data.GetInt32("companyClass").Equals(1);

                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("personClassCode").Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }
                
                p.CompanyName = data.IsDBNull(data.GetOrdinal("companyName")) ? "" : data.GetString("companyName");
                p.Club = data.IsDBNull(data.GetOrdinal("club")) ? "" : data.GetString("club");

                race.AddParticipant(p);
            }

            data.Close();

            ParticipantClass bed = race.Classes.Find(x => x.Id.Equals("BED"));

            // Adding teams
            cmd = new MySqlCommand(@"SELECT t.startNumber, t.chipNumber as cardid, t.name, t.teamClassCode, t.companyClass, t.companyName,  p.firstname, p.surname, p.phoneNumber, p.sprintNumber, p.gender, p.birthyear, p.club FROM " + _year + "_team t inner join " + _year + "_person p on t.id = p.teamid where t.deleted = 0 and t.startNumber is not null and t.chipNumber is not null order by t.startNumber, p.sprintNumber", conn);
            data = cmd.ExecuteReader();

            data.Read();
            bool moreData = true;

            do
            {
                var team = race.Classes.Find(x => x.Id.Equals(data.GetString("teamClassCode")));

                var p = new Participant();

                p.Startnumber = data.GetInt32("startNumber");
                p.EmitID = int.Parse(data.GetString("cardid"));
                p.Name = data.GetString("name");
                p.Telephone = new List<String>() { (data.GetString("phoneNumber") ?? "").Replace(" ", "") };
                p.Classes = new List<ParticipantClass> { team };
                p.IsTeam = true;
                p.IsSuper = false;
                p.IsCompany = data.GetInt32("companyClass").Equals(1);
                p.TeamMembers = new List<string>();
                p.Gender = new List<int>();
                p.Ages = new List<int> { int.Parse(race.Name) - data.GetInt32("birthYear") };
                p.Club = data.IsDBNull(data.GetOrdinal("club")) ? "" : data.GetString("club");

                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("teamClassCode").Substring(0, 2) + "BED")));
                    p.Classes.Add(bed);
                }

                p.CompanyName = data.IsDBNull(data.GetOrdinal("companyName")) ? "" : data.GetString("companyName");

                // Add medlemmer
                while (moreData && data.GetInt32("startNumber").Equals(p.Startnumber))
                {
                    p.TeamMembers.Add(data.GetString("firstName") + " " + data.GetString("surname"));
                    if (!String.IsNullOrEmpty(data.GetString("phoneNumber")))
                        p.Telephone.Add(data.GetString("phoneNumber").Replace(" ", ""));
                    p.Gender.Add(data.GetString("gender").Equals("G") ? 1 : 2);
                    p.Ages.Add(int.Parse(race.Name) - data.GetInt32("birthYear"));
                    moreData = data.Read();
                }

                p.Telephone = p.Telephone.Distinct().ToList<String>();
                race.AddParticipant(p);

            } while (moreData);

            data.Close();
            conn.Close();

            foreach (Participant p in race.Participants)
                race.AddPass(new EmitData()
                {
                    BoxId = 1,
                    Id = p.EmitID,
                    Time = new DateTime(2014, 1, 1, 13, 14, 0)
                });

        }
    }
}
