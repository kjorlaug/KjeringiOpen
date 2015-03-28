using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using EmitReaderLib;

namespace KjeringiData
{
    public class Klasse
    {
        public String Id { get; set; }
        public String Namn { get; set; }
    }

    public class Deltakar
    {
        public Deltakar()
        {
            Passeringar = new List<Passering>();
        }

        public int Startnummer { get; set; }
        public int EmitID { get; set; }

        public String Namn { get; set; }
        public String Telefon { get; set; }

        public Klasse Klasse { get; set; }
        public Boolean Bedrift { get; set; }
        public Boolean Super { get; set; }

        public List<String> Medlemmer { get; set; }
        public List<Passering> Passeringar { get; protected set; }
    }

    public class Passering
    {
        public Plassering Location { get; set; }
        public int EmitID { get; set; }
        public DateTime Tid { get; set; }

    }

    public class Plassering
    {
        public Plassering()
        {
            Resultat = new Stack<ResultatData>();
        }

        public int BoksId { get; set; }
        public Boolean Offisiell { get; set; }
        public String Namn { get; set; }
        public int Sekvens { get; set; }

        public Stack<ResultatData> Resultat { get; protected set; }

    }

    public class Konkurranse
    {
        private Konkurranse()
        {
        }

        private static Konkurranse theInstance;

        public static Konkurranse GetInstance {
            get {
                if (theInstance == null)
                {
                    Reset();
                }
                return theInstance;
            }
        }

        public static void Reset()
        {
            theInstance = new Konkurranse();
            theInstance.Build();
        }

        public ResultatData RegistrerPassering(EmitData data)
        {
            //try
            //{
                Plassering plassering = Plasseringar.Find(x => x.BoksId.Equals(data.BoxId));
                Deltakar deltakar = DeltagarEtterEmitId[data.Id];

                Passering passering = new Passering()
                {
                    EmitID = data.Id,
                    Location = plassering,
                    Tid = data.Time
                };

                // duplikat?
                if (plassering.Offisiell && !data.Test)
                    if (this.Passeringar.Find(p => p.EmitID == passering.EmitID && p.Location.BoksId == passering.Location.BoksId) != null)
                        return null;

                this.Passeringar.Add(passering);
                deltakar.Passeringar.Add(passering);

                // Create resultatdata
                ResultatData d = new ResultatData();

                d.ResultatForEtappe = passering.Location.Namn;
                d.Namn = System.Web.HttpContext.Current.Server.HtmlEncode(deltakar.Namn);
                d.Telefonnummer = deltakar.Telefon;
                d.Startnummer = deltakar.Startnummer.ToString();
                d.EmitID = deltakar.EmitID.ToString();
                d.Offisiell = plassering.Offisiell;

                d.Klasse = System.Web.HttpContext.Current.Server.HtmlEncode(deltakar.Klasse.Namn);

                if (!deltakar.Super)
                {
                    d.Medlemmer = new List<string>();
                    foreach (String s in deltakar.Medlemmer)
                        d.Medlemmer.Add(System.Web.HttpContext.Current.Server.HtmlEncode(s));
                }
                else
                    d.Medlemmer = new List<string>();
                                
                List<Passering> filtert = deltakar.Passeringar.FindAll(x => x.Location.Offisiell).OrderBy(y => y.Tid).ToList<Passering>();
                DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 14, 0);

                d.Etappetider = new List<string>(4);
                d.Etappetider.Add("");
                d.Etappetider.Add("");
                d.Etappetider.Add("");
                d.Etappetider.Add("");
                TimeSpan totalTid = new TimeSpan(0);
                foreach (Passering pass in filtert) { 
                    d.Etappetider[pass.Location.Sekvens - 1] = (pass.Tid - start).ToString(@"hh\:mm\:ss");
                    totalTid += (pass.Tid - start);
                    start = pass.Tid;
                }

                d.TotalTid = totalTid.ToString(@"hh\:mm\:ss");
               
                List<Deltakar> KlasseListe = DeltakarListeEtterKlasse[deltakar.Klasse].FindAll(x => x.Passeringar.Exists(y => y.Location.BoksId == passering.Location.BoksId)).ToList<Deltakar>();
                KlasseListe = KlasseListe.OrderBy(x => x.Passeringar.Find(y => y.Location.BoksId == passering.Location.BoksId).Tid).ToList<Deltakar>();

                d.PlasseringIKlasse = KlasseListe.FindIndex(x => x.EmitID == deltakar.EmitID) + 1;
                d.Test = data.Test;

                if(!data.Test)
                    plassering.Resultat.Push(d);
                
                return d;
            //}
            //catch (Exception ex)
            //{
            //    // Something went wrong...
            //    return null;
            //}
        }

        // Build content in singleton
        protected void Build() {
            Passeringar = new List<Passering>();
            
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["kjeringi"].ConnectionString);

            conn.Open();

            // Read klasser
            SqlCommand cmd = new SqlCommand(@"SELECT id, name, official, sequence FROM kop_station", conn);
            Plasseringar = new List<Plassering>();

            SqlDataReader data = cmd.ExecuteReader();

            while (data.Read())
            {
                Plasseringar.Add(new Plassering()
                {
                    BoksId = data.GetInt32(data.GetOrdinal("id")),
                    Namn = data.GetString(data.GetOrdinal("name")),
                    Offisiell = data.GetInt32(data.GetOrdinal("official")).Equals(1),
                    Sekvens = data.GetInt32(data.GetOrdinal("sequence"))
                });
            }
            data.Close();

            // Read klasser
            cmd = new SqlCommand(@"SELECT code, name FROM kop_personClass union SELECT code, name FROM kop_teamClass", conn);
            Klassar = new List<Klasse>();

            data = cmd.ExecuteReader();

            while (data.Read())
            {
                Klassar.Add(new Klasse()
                {
                    Id = data.GetString(data.GetOrdinal("code")),
                    Namn = data.GetString(data.GetOrdinal("name"))
                });
            }
            data.Close();

            Deltagarar = new List<Deltakar>();
            DeltagarEtterEmitId = new Dictionary<int, Deltakar>();
            DeltakarListeEtterKlasse = new Dictionary<Klasse, List<Deltakar>>();

            // Adding supers
            cmd = new SqlCommand(@"select startNumber, chipNumber, firstname, surname, personClassCode, phoneNumber from kop_person where superwife = 1 and deleted = 0 and startnumber is not null and chipnumber is not null", conn);
            data = cmd.ExecuteReader();

            while (data.Read())
            {
                var d = new Deltakar()
                {
                    Startnummer = data.GetInt32(data.GetOrdinal("startNumber")),
                    EmitID = int.Parse(data.GetString(data.GetOrdinal("chipNumber"))),
                    Namn = data.GetString(data.GetOrdinal("firstname")) + " " + data.GetString(data.GetOrdinal("surname")),
                    Telefon = data.GetString(data.GetOrdinal("phoneNumber")),
                    Klasse = Klassar.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("personClassCode")))),
                    Bedrift = false,
                    Super = true
                };

                if (!DeltakarListeEtterKlasse.ContainsKey(d.Klasse))
                    DeltakarListeEtterKlasse.Add(d.Klasse, new List<Deltakar>());

                DeltakarListeEtterKlasse[d.Klasse].Add(d);
                DeltagarEtterEmitId.Add(d.EmitID, d);
            }

            data.Close();

            cmd = new SqlCommand(@"SELECT t.startNumber, t.chipNumber, t.name, t.teamClassCode, t.companyClass, p.firstname, p.surname, p.phoneNumber, p.sprintNumber FROM kop_team t inner join kop_person p on t.id = p.teamid where t.deleted = 0 and t.startNumber is not null and t.chipNumber is not null order by t.startNumber, p.sprintNumber", conn);
            data = cmd.ExecuteReader();

            data.Read();
            bool moreData = true;

            do
            {
                var d = new Deltakar()
                {
                    Startnummer = data.GetInt32(data.GetOrdinal("startNumber")),
                    EmitID = int.Parse(data.GetString(data.GetOrdinal("chipNumber"))),
                    Namn = data.GetString(data.GetOrdinal("name")),
                    Klasse = Klassar.Find(x => x.Id.Equals(data.GetString(data.GetOrdinal("teamClassCode")))),
                    Bedrift = data.GetInt32(data.GetOrdinal("companyClass")).Equals(1),
                    Super = false
                };

                // Add medlemmer
                d.Medlemmer = new List<string>();
                while (moreData && data.GetInt32(data.GetOrdinal("startNumber")).Equals(d.Startnummer))
                {
                    d.Medlemmer.Add(data.GetString(data.GetOrdinal("firstName")) + " " + data.GetString(data.GetOrdinal("surname")));
                    if (!String.IsNullOrEmpty(data.GetString(data.GetOrdinal("phoneNumber"))))
                        d.Telefon += String.IsNullOrEmpty(d.Telefon) ? data.GetString(data.GetOrdinal("phoneNumber")) : "," + data.GetString(data.GetOrdinal("phoneNumber"));
                    moreData = data.Read();
                }

                if (!DeltakarListeEtterKlasse.ContainsKey(d.Klasse))
                    DeltakarListeEtterKlasse.Add(d.Klasse, new List<Deltakar>());

                DeltakarListeEtterKlasse[d.Klasse].Add(d);
                DeltagarEtterEmitId.Add(d.EmitID, d);

            } while (moreData);

            data.Close();

            // Load any times registered
            cmd = new SqlCommand("SELECT card, time, location FROM timers_raw WHERE year = 14 ORDER BY id", conn);
            data = cmd.ExecuteReader();
            
            while (data.Read()) {
                DateTime time = DateTime.ParseExact(data.GetString(data.GetOrdinal("time")), "HH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
                try
                {
                    RegistrerPassering(
                        new EmitData()
                        {
                            Id = (int)data[data.GetOrdinal("card")],
                            BoxId = (int)data[data.GetOrdinal("location")],
                            Time = time
                        });
                }
                catch (Exception ex) { }
            }
            data.Close();
            conn.Close();
        }

        public List<Klasse> Klassar { get; protected set; }
        public List<Deltakar> Deltagarar { get; protected set; }
        public List<Plassering> Plasseringar { get; protected set; }

        public List<Passering> Passeringar { get; protected set; }

        public Dictionary<int, Deltakar> DeltagarEtterEmitId { get; protected set; }
        public Dictionary<Klasse, List<Deltakar>> DeltakarListeEtterKlasse { get; protected set; }
    }

    public class ResultatData
    {
        public String ResultatForEtappe { get; set; }
        public Boolean Offisiell { get; set; }

        public String Namn { get; set; }
        public String Telefonnummer { get; set; }

        public String EmitID { get; set; }
        public String Startnummer { get; set; }

        public List<String> Medlemmer { get; set; }
        public List<String> Etappetider { get; set; }

        public String TotalTid { get; set; }

        public String Klasse {get;set;}
        public int PlasseringIKlasse { get; set; }

        public Boolean Test { get; set; }
    }
}
