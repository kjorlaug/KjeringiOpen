using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace KjeringiData
{
    public class Klasse
    {
        public String Id { get; set; }
        public String Namn { get; set; }
    }

    public class Deltakar
    {
        public int Startnummer { get; set; }
        public int EmitID { get; set; }

        public String Namn { get; set; }
        public String Telefon { get; set; }

        public Klasse Klasse { get; set; }
        public Boolean Bedrift { get; set; }
        public Boolean Super { get; set; }

        public List<String> Medlemmer { get; set; }
        protected List<Passering> Passeringar { get; set; }
    }

    public class Passering
    {
        public Plassering Location { get; set; }
        public int EmitID { get; set; }
        public DateTime Tid { get; set; }
    }

    public class Plassering
    {
        public int BoksId { get; set; }
        public Boolean Offisiell { get; set; }
        public String Namn { get; set; }
        public int Sekvens { get; set; }
    }

    public class Konkurranse
    {
        private Konkurranse()
        {
        }

        private static Konkurranse theInstance;

        public static Konkurranse GetInstance {
            get {
                /*if (theInstance == null)
                {*/
                    theInstance = new Konkurranse();
                    theInstance.Build();
                //}
                return theInstance;
            }
        }

        // Build content in singleton
        protected void Build() {
            MySqlConnection conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["kjeringi"].ConnectionString);

            conn.Open();

            // Read klasser
            MySqlCommand cmd = new MySqlCommand(@"SELECT id, name, official, sequence FROM kop_station", conn);
            Plasseringar = new List<Plassering>();

            MySqlDataReader data = cmd.ExecuteReader();

            while (data.Read())
            {
                Plasseringar.Add(new Plassering()
                {
                    BoksId = data.GetInt32("id"),
                    Namn = data.GetString("name"),
                    Offisiell = data.GetInt32("official").Equals(1),
                    Sekvens = data.GetInt32("sequence")
                });
            }
            data.Close();

            // Read klasser
            cmd = new MySqlCommand(@"SELECT code, name FROM kop_personClass union SELECT code, name FROM kop_teamClass", conn);
            Klassar = new List<Klasse>();

            data = cmd.ExecuteReader();

            while (data.Read())
            {
                Klassar.Add(new Klasse()
                {
                    Id = data.GetString("code"),
                    Namn = data.GetString("name")
                });
            }
            data.Close();

            Deltagarar = new List<Deltakar>();
            DeltagarEtterEmitId = new Dictionary<int, Deltakar>();
            DeltakarListeEtterKlasse = new Dictionary<Klasse, List<Deltakar>>();

            // Adding supers
            cmd = new MySqlCommand(@"select startNumber, chipNumber, firstname, surname, personClassCode, phoneNumber from kop_person where superwife = 1 and deleted = 0", conn);
            data = cmd.ExecuteReader();

            while (data.Read())
            {
                var d = new Deltakar()
                {
                    Startnummer = data.GetInt32("startNumber"),
                    EmitID = data.GetInt32("chipNumber"),
                    Namn = data.GetString("firstname") + " " + data.GetString("surname"),
                    Telefon = data.GetString("phoneNumber"),
                    Klasse = Klassar.Find(x => x.Id.Equals(data.GetString("personClassCode"))),
                    Bedrift = false,
                    Super = true
                };

                if (!DeltakarListeEtterKlasse.ContainsKey(d.Klasse))
                    DeltakarListeEtterKlasse.Add(d.Klasse, new List<Deltakar>());

                DeltakarListeEtterKlasse[d.Klasse].Add(d);
                DeltagarEtterEmitId.Add(d.EmitID, d);
            }

            data.Close();

            cmd = new MySqlCommand(@"SELECT t.startNumber, t.chipNumber, t.name, t.teamClassCode, t.companyClass, p.firstname, p.surname, p.phoneNumber, p.sprintNumber FROM kop_team t inner join kop_person p on t.id = p.teamid where t.deleted = 0 order by t.startNumber, p.sprintNumber", conn);
            data = cmd.ExecuteReader();

            data.Read();
            bool moreData = true;

            do
            {
                var d = new Deltakar()
                {
                    Startnummer = data.GetInt32("startNumber"),
                    EmitID = data.GetInt32("chipNumber"),
                    Namn = data.GetString("name"),
                    Klasse = Klassar.Find(x => x.Id.Equals(data.GetString("teamClassCode"))),
                    Bedrift = data.GetInt32("companyClass").Equals(1),
                    Super = false
                };

                // Add medlemmer
                d.Medlemmer = new List<string>();
                while (moreData && data.GetInt32("startNumber").Equals(d.Startnummer))
                {
                    d.Medlemmer.Add(data.GetString("firstName") + " " + data.GetString("surname"));
                    if (!String.IsNullOrEmpty(data.GetString("phoneNumber")))
                        d.Telefon += String.IsNullOrEmpty(d.Telefon) ? data.GetString("phoneNumber") : "," + data.GetString("phoneNumber");
                    moreData = data.Read();
                }

                if (!DeltakarListeEtterKlasse.ContainsKey(d.Klasse))
                    DeltakarListeEtterKlasse.Add(d.Klasse, new List<Deltakar>());

                DeltakarListeEtterKlasse[d.Klasse].Add(d);
                DeltagarEtterEmitId.Add(d.EmitID, d);

            } while (moreData);

            data.Close();

            conn.Close();
        }

        public List<Klasse> Klassar { get; protected set; }
        public List<Deltakar> Deltagarar { get; protected set; }
        public List<Plassering> Plasseringar { get; protected set; }

        //public List<Passering> Passeringar { get; }

        public Dictionary<int, Deltakar> DeltagarEtterEmitId { get; protected set; }
        public Dictionary<Klasse, List<Deltakar>> DeltakarListeEtterKlasse { get; protected set; }
    }

}
