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
        public MySqlRaceBuilder2015(String connection, List<int> testers, String year, DateTime raceDate)
        {
            _conn = connection;
            _year = year;
            Testers = testers;
        }

        protected String _conn { get; set; }
        protected String _year{ get; set; }
        protected DateTime _raceDate { get; set; }
        protected List<int> Testers { get; set; }

        public void BuildRace(Race race)
        {
            race.Name = _year;

            MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[_conn].ConnectionString);
            conn.Open();

            // Adding supers
            var cmd = new MySqlCommand(@"select 
	                kop_person.startNumber, kop_person.chipNumber, kop_person.firstname, ifnull(kop_person.tshirtName, '') as tshirtName, kop_person.surname, kop_person.personClassCode, ifnull(kop_person.companyClass, ifnull(kop_team.companyClass, 0)) as companyClass, kop_team.companyName, kop_person.phoneNumber, kop_person.club,
                    case 
	                    when cupClass = 1 and (2015 - birthyear) in (10, 11,12) then concat('NM', gender, '11')
	                    when cupClass = 1 and (2015 - birthyear) in (13,14) then concat('NM', gender, '13')
	                    when cupClass = 1 and (2015 - birthyear) in (15,16) then concat('NM', gender, '15')
	                    when cupClass = 1 and (2015 - birthyear) in (17,18,19,20,21) then concat('NM', gender, '17')
	                    when cupClass = 1 and (2015 - birthyear) > 21 then concat('NM', gender, '22')
	                    else null
                    end as cupClass
                from 
	                kop_person left join kop_team on kop_person.teamid = kop_team.id where superwife = 1 and kop_person.deleted = 0 and kop_person.startnumber is not null and kop_person.chipnumber is not null
                order by startnumber", conn);

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
                    IsCompany = data.GetInt32("companyClass").Equals(1),
                    CompanyName = data.IsDBNull(data.GetOrdinal("club")) ? "": data.GetString("club"),
                    ShirtSizes = new List<String>() { data.GetString("tShirtName")}                 
                };
                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("personClassCode").Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }
                if (!data.IsDBNull(data.GetOrdinal("club")) && !String.IsNullOrWhiteSpace(data.GetString("club")))
                {
                    var cn = data.GetString("club");
                    if (cn.StartsWith("SVV"))
                        cn = "SVV";
                    if (!race.Classes.Exists(x => x.Id.Equals(cn)))
                        race.Classes.Add(new ParticipantClass() { Id = cn, Official = false, Name = cn, Sequence = race.Classes.Count + 1 });
                    
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(cn)));
                }
                if (!data.IsDBNull(data.GetOrdinal("cupClass")))
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("cupClass"))));

                race.AddParticipant(p);
            }
           
            data.Close();

            // Add NM Cup
            cmd = new MySqlCommand(@"select 
	                startNumber, chipNumber, firstname, surname, personClassCode, ifnull(companyClass, 0) as companyClass, companyName, phoneNumber, 
                case 
	                when cupClass = 1 and (2015 - birthyear) in (10, 11,12) then concat('NM', gender, '11')
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
                    IsCompany = data.GetInt32("companyClass").Equals(1),
                    CompanyName = data.IsDBNull(data.GetOrdinal("companyName")) ? "" : data.GetString("companyName")
                };
                if (p.IsCompany)
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(data.GetString("personClassCode").Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }
                if (!data.IsDBNull(data.GetOrdinal("companyName")) && !String.IsNullOrWhiteSpace(data.GetString("companyName")))
                {
                    var cn = data.GetString("companyName");
                    if (cn.StartsWith("SVV"))
                        cn = "SVV";

                    if (!race.Classes.Exists(x => x.Id.Equals(cn)))
                        race.Classes.Add(new ParticipantClass() { Id = cn, Official = false, Name = cn, Sequence = race.Classes.Count + 1 });

                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(cn)));
                }

                race.AddParticipant(p);
            }

            data.Close();

            // Adding teams
            cmd = new MySqlCommand(@"SELECT t.startNumber, t.chipNumber, t.name, t.teamClassCode, t.companyClass, p.firstname, p.surname, p.phoneNumber, p.sprintNumber, ifnull(p.tshirtName, '') as tshirtName FROM kop_team t inner join kop_person p on t.id = p.teamid where t.deleted = 0 and t.startNumber is not null and t.chipNumber is not null order by t.startNumber, p.sprintNumber", conn);

            data = cmd.ExecuteReader();
            
            bool moreData = data.Read();

            if (moreData)
            {
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
                    if (p.Name.StartsWith("SVV"))
                    {
                        String cn = "SVV";
                        if (!race.Classes.Exists(x => x.Id.Equals(cn)))
                            race.Classes.Add(new ParticipantClass() { Id = cn, Official = false, Name = cn, Sequence = race.Classes.Count + 1 });

                        p.Classes.Add(race.Classes.Find(x => x.Id.Equals(cn)));
                    }
                    p.TeamMembers = new List<string>();
                    p.ShirtSizes = new List<string>();

                    // Add medlemmer
                    while (moreData && data.GetInt32("startNumber").Equals(p.Startnumber))
                    {
                        p.TeamMembers.Add(data.GetString("firstName") + " " + data.GetString("surname"));
                        p.ShirtSizes.Add(data.GetString("tshirtName"));
                        if (!String.IsNullOrEmpty(data.GetString("phoneNumber")))
                            p.Telephone.Add(data.GetString("phoneNumber").Replace(" ", ""));
                        moreData = data.Read();
                    }

                    p.Telephone = p.Telephone.Distinct().ToList<String>();
                    race.AddParticipant(p);

                } while (moreData);
            }

            data.Close();
            conn.Close();

            // Add testers
            foreach (int testId in Testers)
            {
                var parTest = new Participant()
                {
                    Startnumber = testId,
                    EmitID = testId,
                    Name = "Test " + testId.ToString(),
                    Telephone = new List<String>() { "95116354", "95246298", "", "41530965", "48021455" },
                    Classes = new List<ParticipantClass>() { race.Classes.Find(x => x.Id.Equals("TEST")) },
                    IsTeam = true,
                    TeamMembers = new List<String>() { "Rune Kjørlaug", "Petter Stenstavold", "Erlend Klakegg Bergheim", "Even Østvold" },
                    ShirtSizes = new List<string>() {"m", "m", "m", "m"},
                    IsSuper = false,
                    IsCompany = false                    
                };
                race.AddParticipant(parTest);
            }

            race.Testers = Testers;

            DateTime start = new DateTime(_raceDate.Year, _raceDate.Month, _raceDate.Day, 13, 14, 0);

            foreach (Participant p in race.Participants)
                race.AddPass(new EmitData()
                {
                    BoxId = 1,
                    Id = p.EmitID,
                    Time = start
                });
        }
    }
}
