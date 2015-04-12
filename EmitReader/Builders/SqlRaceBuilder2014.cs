using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using System.Data.SqlClient;

namespace EmitReaderLib.Builders
{
    public class SqlRaceBuilder2014 : IRaceBuilder
    {
        public void BuildRace(Race race)
        {
            race.Name = "KjeringiOpen 2014";

            SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["kjeringi"].ConnectionString);
            conn.Open();

            // Read klasser
            SqlCommand cmd = new SqlCommand(@"SELECT id, name, official, sequence FROM kop_station", conn);
            SqlDataReader data = cmd.ExecuteReader();

            while (data.Read())
            {

                race.TimeStations.Add(new TimeStation()
                {
                    Id = data.GetInt32(data.GetOrdinal("id")),
                    Name = data.GetString(data.GetOrdinal("name")),
                    Official = data.GetInt32(data.GetOrdinal("official")).Equals(1),
                    Sequence = data.GetInt32(data.GetOrdinal("sequence"))
                });
            }
            data.Close();

            // Read klasser
            cmd = new SqlCommand(@"SELECT code, name FROM kop_personClass union SELECT code, name FROM kop_teamClass", conn);
            data = cmd.ExecuteReader();

            while (data.Read())
            {
                race.Classes.Add(new ParticipantClass() {
                    Id = data.GetString(data.GetOrdinal("code")),
                    Name = data.GetString(data.GetOrdinal("name")),
                    Sequence = 1
                });
            }
            data.Close();

            // Add static classes
            var companyClass = new ParticipantClass { Id = "BED", Name = "Bedriftsklasse", Sequence = 100 };
            var testClass = new ParticipantClass { Id = "TEST", Name = "Testklasse", Sequence = 101 };

            race.Classes.Add(companyClass);
            race.Classes.Add(testClass);

            // Adding supers
            cmd = new SqlCommand(@"select startNumber, chipNumber, firstname, surname, personClassCode, phoneNumber from kop_person where superwife = 1 and deleted = 0 and startnumber is not null and chipnumber is not null", conn);
            data = cmd.ExecuteReader();

            while (data.Read())
            {
                race.AddParticipant(new Participant()
                {
                    Startnumber = data.GetInt32(data.GetOrdinal("startNumber")),
                    EmitID = int.Parse(data.GetString(data.GetOrdinal("chipNumber"))),
                    Name = data.GetString(data.GetOrdinal("firstname")) + " " + data.GetString(data.GetOrdinal("surname")),
                    Telephone = new List<String>() {data.GetString(data.GetOrdinal("phoneNumber"))},
                    Classes = new List<ParticipantClass>() {race.Classes.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("personClassCode"))))},
                    IsTeam = false,
                    IsSuper = true,
                    IsBusiness = false
                });                
            }

            data.Close();

            cmd = new SqlCommand(@"SELECT t.startNumber, t.chipNumber, t.name, t.teamClassCode, t.companyClass, p.firstname, p.surname, p.phoneNumber, p.sprintNumber FROM kop_team t inner join kop_person p on t.id = p.teamid where t.deleted = 0 and t.startNumber is not null and t.chipNumber is not null order by t.startNumber, p.sprintNumber", conn);
            data = cmd.ExecuteReader();

            data.Read();
            bool moreData = true;

            do
            {
                var p = new Participant()
                {
                    Startnumber = data.GetInt32(data.GetOrdinal("startNumber")),
                    EmitID = int.Parse(data.GetString(data.GetOrdinal("chipNumber"))),
                    Name = data.GetString(data.GetOrdinal("firstname")) + " " + data.GetString(data.GetOrdinal("surname")),
                    Telephone = new List<String>() {data.GetString(data.GetOrdinal("phoneNumber"))},
                    Classes = new List<ParticipantClass> {race.Classes.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("teamClassCode"))))},
                    IsTeam = true,
                    IsSuper = false,
                    IsBusiness = data.GetInt32(data.GetOrdinal("companyClass")).Equals(1)
                };

                if (p.IsBusiness)
                    p.Classes.Add(companyClass);

                // Add medlemmer
                while (moreData && data.GetInt32(data.GetOrdinal("startNumber")).Equals(p.Startnumber))
                {
                    p.TeamMembers.Add(data.GetString(data.GetOrdinal("firstName")) + " " + data.GetString(data.GetOrdinal("surname")));
                    if (!String.IsNullOrEmpty(data.GetString(data.GetOrdinal("phoneNumber"))))
                        p.Telephone.Add(data.GetString(data.GetOrdinal("phoneNumber")));
                    moreData = data.Read();
                }

                race.AddParticipant(p);

            } while (moreData);

            data.Close();
            conn.Close();

        }
    }
}
