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

            MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[_conn].ConnectionString);
            conn.Open();

            // Adding supers
            var cmd = new MySqlCommand(@"select startNumber, chipNumber, firstname, surname, personClassCode, companyClass, phoneNumber from kop_person where superwife = 1 and deleted = 0 and startnumber is not null and chipnumber is not null", conn);
            var data = cmd.ExecuteReader();

            while (data.Read())
            {
                var p = new Participant()
                {
                    Startnumber = data.GetInt32(data.GetOrdinal("startNumber")),
                    EmitID = int.Parse(data.GetString(data.GetOrdinal("chipNumber"))),
                    Name = data.GetString(data.GetOrdinal("firstname")) + " " + data.GetString(data.GetOrdinal("surname")),
                    Telephone = new List<String>() { data.GetString(data.GetOrdinal("phoneNumber")) },
                    Classes = new List<ParticipantClass>() { race.Classes.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("personClassCode")))) },
                    IsTeam = false,
                    IsSuper = true,
                    IsCompany = data.GetInt32(data.GetOrdinal("companyClass")).Equals(1)
                };
                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("personClassCode")).Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }
                race.AddParticipant(p);
            }
           
            data.Close();

            // Adding teams
            cmd = new MySqlCommand(@"SELECT t.startNumber, t.chipNumber, t.name, t.teamClassCode, t.companyClass, p.firstname, p.surname, p.phoneNumber, p.sprintNumber FROM kop_team t inner join kop_person p on t.id = p.teamid where t.deleted = 0 and t.startNumber is not null and t.chipNumber is not null order by t.startNumber, p.sprintNumber", conn);
            data = cmd.ExecuteReader();

            data.Read();
            bool moreData = true;

            do
            {
                var p = new Participant()
                {
                    Startnumber = data.GetInt32(data.GetOrdinal("startNumber")),
                    EmitID = int.Parse(data.GetString(data.GetOrdinal("chipNumber"))),
                    Name = data.GetString(data.GetOrdinal("name")),
                    Telephone = new List<String>() { data.GetString(data.GetOrdinal("phoneNumber")).Replace(" ", "") },
                    Classes = new List<ParticipantClass> { race.Classes.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("teamClassCode")))) },
                    IsTeam = true,
                    IsSuper = false,
                    IsCompany = data.GetInt32(data.GetOrdinal("companyClass")).Equals(1)
                };

                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("teamClassCode")).Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }

                // Add medlemmer
                while (moreData && data.GetInt32(data.GetOrdinal("startNumber")).Equals(p.Startnumber))
                {
                    p.TeamMembers.Add(data.GetString(data.GetOrdinal("firstName")) + " " + data.GetString(data.GetOrdinal("surname")));
                    if (!String.IsNullOrEmpty(data.GetString(data.GetOrdinal("phoneNumber"))))
                        p.Telephone.Add(data.GetString(data.GetOrdinal("phoneNumber")).Replace(" ", ""));
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
                    Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 14, 0)
                });
        }
    }
}
