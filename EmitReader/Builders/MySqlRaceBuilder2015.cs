using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using MySql.Data.MySqlClient;

namespace EmitReaderLib.Builders
{
    public class MySqlRaceBuilder2015 : IRaceBuilder
    {
        public MySqlRaceBuilder2015(String connection)
        {
            _conn = connection;
            _year = "2015";
        }

        protected String _conn { get; set; }
        protected String _year{ get; set; }

        public void BuildRace(Race race)
        {
            race.Name = _year;

            MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[_conn].ConnectionString);
            conn.Open();

            // Adding supers
            var cmd = new MySqlCommand(@"select 
	                startNumber, chipNumber, firstname, surname, personClassCode, ifnull(companyClass, 0) as companyClass, phoneNumber, 
                case 
	                when cupClass = 1 and (2015 - birthyear) in (11,12) then concat('NM', gender, '11')
	                when cupClass = 1 and (2015 - birthyear) in (13,14) then concat('NM', gender, '13')
	                when cupClass = 1 and (2015 - birthyear) in (15,16) then concat('NM', gender, '15')
	                when cupClass = 1 and (2015 - birthyear) in (17,18,19,20,21) then concat('NM', gender, '17')
	                when cupClass = 1 and (2015 - birthyear) > 21 then concat('NM', gender, '22')
	                else null
                end as cupClass
                from kop_person where superwife = 1 and deleted = 0 and startnumber is not null and chipnumber is not null", conn);

            var data = cmd.ExecuteReader();

            while (data.Read())
            {
                var p = new Participant()
                {
                    Startnumber = data.GetInt32("startNumber"),
                    EmitID = int.Parse(data.GetString("chipNumber")),
                    Name = data.GetString("firstname") + " " + data.GetString("surname"),
                    Telephone = new List<String>() { data.GetString("phoneNumber") },
                    Classes = new List<ParticipantClass>() { race.Classes.Find(x => x.Id.Equals(data.GetString("personClassCode"))) },
                    IsTeam = false,
                    IsSuper = true,
                    IsCompany = data.GetInt32("companyClass").Equals(1)
                };
                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("personClassCode").Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }
                if (!data.IsDBNull(data.GetOrdinal("cupClass")))
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("cupClass"))));

                race.AddParticipant(p);
            }
           
            data.Close();

            // Add NM Cup
            cmd = new MySqlCommand(@"select 
	                startNumber, chipNumber, firstname, surname, personClassCode, ifnull(companyClass, 0) as companyClass, phoneNumber, 
                case 
	                when cupClass = 1 and (2015 - birthyear) in (11,12) then concat('NM', gender, '11')
	                when cupClass = 1 and (2015 - birthyear) in (13,14) then concat('NM', gender, '13')
	                when cupClass = 1 and (2015 - birthyear) in (15,16) then concat('NM', gender, '15')
	                when cupClass = 1 and (2015 - birthyear) in (17,18,19,20,21) then concat('NM', gender, '17')
	                when cupClass = 1 and (2015 - birthyear) > 21 then concat('NM', gender, '22')
	                else null
                end as cupClass
                from kop_person where superwife = 0 and cupClass = 1 and deleted = 0 and startnumber is not null and chipnumber is not null", conn);

            data = cmd.ExecuteReader();

            while (data.Read())
            {
                var p = new Participant()
                {
                    Startnumber = data.GetInt32("startNumber"),
                    EmitID = int.Parse(data.GetString("chipNumber")),
                    Name = data.GetString("firstname") + " " + data.GetString("surname"),
                    Telephone = new List<String>() { data.GetString("phoneNumber") },
                    Classes = new List<ParticipantClass>() { race.Classes.Find(x => x.Id.Equals(data.GetString("cupClass"))) },
                    IsTeam = false,
                    IsSuper = true,
                    IsCompany = data.GetInt32("companyClass").Equals(1)
                };
                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("personClassCode").Substring(0, 2) + "BED")));
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
                    Startnumber = data.GetInt32("startNumber"),
                    EmitID = int.Parse(data.GetString("chipNumber")),
                    Name = data.GetString("name"),
                    Telephone = new List<String>() { data.GetString("phoneNumber").Replace(" ", "") },
                    Classes = new List<ParticipantClass> { race.Classes.Find(x => x.Id.Equals(data.GetString("teamClassCode"))) },
                    IsTeam = true,
                    IsSuper = false,
                    IsCompany = data.GetInt32("companyClass").Equals(1)
                };

                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("teamClassCode").Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }

                // Add medlemmer
                while (moreData && data.GetInt32("startNumber").Equals(p.Startnumber))
                {
                    p.TeamMembers.Add(data.GetString("firstName") + " " + data.GetString("surname"));
                    if (!String.IsNullOrEmpty(data.GetString("phoneNumber")))
                        p.Telephone.Add(data.GetString("phoneNumber").Replace(" ", ""));
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
