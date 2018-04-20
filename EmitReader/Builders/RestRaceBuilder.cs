using EmitReaderLib.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Builders
{
    public class RestRaceBuilder : IRaceBuilder
    {
        public RestRaceBuilder(String connection, List<int> testers, String year, DateTime raceDate)
        {
            _conn = connection;
            _year = year;
            Testers = testers;
            _raceDate = raceDate;
        }

        protected String _conn { get; set; }
        protected String _year { get; set; }
        protected DateTime _raceDate { get; set; }
        protected List<int> Testers { get; set; }

        public void BuildRace(Race race)
        {
            // Should load the complete model from persisted state
            //var blob = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=kjeringi2018;AccountKey=Ex59J8C12E//TwKjCA2tQWOi9ddCRZ6wRcjGR2o34h/pU9q+7Gq+7+lNbLMQsujhclbLtTnfO7+1PflJzUk/5Q==;EndpointSuffix=core.windows.net");

            //CloudBlobClient blobClient = blob.CreateCloudBlobClient();
            //CloudBlobContainer container = blobClient.GetContainerReference("racedata");

            //if (!container.Exists())
            //    container.Create();

            //CloudBlockBlob blockBlob = container.GetBlockBlobReference(_year);
            //if (blockBlob.Exists())
            //{
            //    List<Participant> par = JsonConvert.DeserializeObject<List<Participant>>(blockBlob.DownloadText());
            //    foreach (Participant p in par)
            //        race.AddParticipant(p);
            //}

            // Sync state with external system
            var supers = CallRestService("https://www.skriki.no/kop18/ipa/allsuper/fetchAll?token=7c24773044681dd6c8c435f726269364f549698436504d09c382639841c408a3", "GET");

            foreach (dynamic t in supers.Children<JObject>())
            {
                var p = new Participant()
                {
                    Id = t.id,
                    Updated = (DateTime) t.updated,
                    Startnumber = t.startNumber == null ? -(int)t.id : (int)t.startNumber,
                    EmitID = t.chipNumber == null || t.chipNumber == 0 ? -(int)t.id - 1000 : (int)t.chipNumber,
                    Name = PrettyPrintName(t.firstname.ToString() + " " + t.surname.ToString()),
                    Telephone = new List<String>() { CleanMobile(t.phone.ToString()) },
                    Classes = new List<ParticipantClass>(),
                    IsTeam = false,
                    IsSuper = true,
                    IsCompany = false,
                    Ages = new List<int> { int.Parse(_year) - (int)t.birthYear},
                    Gender = new List<int> { t.gender.ToString().Equals("J") ? 2 : 1 },
                    CompanyName = t.companyName ?? "",
                    ShirtSizes = new List<String>() { (string)t.tshirtCode ?? "" }
                };

                /* Add logic on NM */
                if (t.norwaycup == 1)
                {
                    var age = int.Parse(this._year) - (int)t.birthYear;
                    var ageGroup = 22;

                    if (age <= 12)
                        ageGroup = 11;
                    else if (age <= 14)
                        ageGroup = 13;
                    else if (age <= 16)
                        ageGroup = 15;
                    else if (age <= 21)
                        ageGroup = 17;
                    else if (age >= 40)
                        ageGroup = 40;

                    var cn = "NM" + t.gender + ageGroup.ToString();
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(cn)));
                }

                if (t.notASuper.ToString().Equals("0")) {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals((string)t.classCode)));

                    if (!String.IsNullOrEmpty(p.CompanyName))
                    {
                        p.Classes.Add(race.Classes.Find(x => x.Id.Equals(t.classCode.ToString().Substring(0, 2) + "BED")));
                    }
                    if (!String.IsNullOrWhiteSpace(t.club.ToString()))
                    {
                        var cn = t.club.ToString();
                        if (cn.StartsWith("SVV") || cn.ToLower().StartsWith("vegil"))
                            cn = "SVV";
                        if (!race.Classes.Exists(x => x.Id.Equals(cn)))
                            race.Classes.Add(new ParticipantClass() { Id = cn, Official = false, Name = cn, Sequence = race.Classes.Count + 1 });

                        p.Classes.Add(race.Classes.Find(x => x.Id.Equals(cn)));
                    }
                }

                if (p.EmitID == 0)
                    throw new IndexOutOfRangeException();

                if (p.Startnumber > 0)
                    race.AddOrUpdateParticipant(p);
            }

            // Sync state with external system
            var teams = CallRestService("https://www.skriki.no/kop18/ipa/team/fetchAll?token=7c24773044681dd6c8c435f726269364f549698436504d09c382639841c408a3", "GET");

            foreach (dynamic t in teams.Children<JObject>())
            {
                var p = new Participant()
                {
                    Id = (int)t.id + 2000,
                    Updated = (DateTime)t.updated,
                    Startnumber = t.startNumber == null || t.startNumber == 0 ? -(int)t.id : (int)t.startNumber,
                    EmitID = t.chipNumber == null || t.chipNumber == 0 ? -(int)t.id - 5000 : (int)t.chipNumber,
                    Name = t.name,
                    Telephone = new List<String>(),
                    Classes = new List<ParticipantClass>() { race.Classes.Find(x => x.Id.Equals((string)t.classCode)) },
                    IsTeam = true,
                    IsSuper = false,
                    IsCompany = false,
                    CompanyName = t.companyName ?? "",
                    TeamMembers = new List<String>(),
                    ShirtSizes = new List<String>(),
                    Ages = new List<int>(),
                    Gender = new List<int>()
                };

                if (p.EmitID == 0)
                    throw new IndexOutOfRangeException();

                foreach (dynamic m in t.members)
                {
                    p.TeamMembers.Add(PrettyPrintName((string)m.firstname + " " + (string)m.surname));
                    p.Ages.Add(int.Parse(_year) - (int)m.birthYear);
                    p.Gender.Add(m.gender.ToString().Equals("J") ? 2 : 1);
                    p.Telephone.Add(CleanMobile((string)m.phone));
                    p.ShirtSizes.Add((string)m.tshirtCode);
                }

                if (!String.IsNullOrEmpty(p.CompanyName))
                {
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(t.classCode.ToString().Substring(0, 2) + "BED")));
                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals("BED")));
                }
                if (!String.IsNullOrWhiteSpace(t.club.ToString()))
                {
                    var cn = t.club.ToString();
                    if (cn.StartsWith("SVV") || cn.ToLower().StartsWith("vegil"))
                        cn = "SVV";
                    if (!race.Classes.Exists(x => x.Id.Equals(cn)))
                        race.Classes.Add(new ParticipantClass() { Id = cn, Official = false, Name = cn, Sequence = race.Classes.Count + 1 });

                    p.Classes.Add(race.Classes.Find(x => x.Id.Equals(cn)));
                }

                race.AddOrUpdateParticipant(p);

            }

            // Add testers
            foreach (int testId in Testers)
            {
                var parTest = new Participant()
                {
                    Id = -testId,
                    Updated = DateTime.Now,
                    Startnumber = testId,
                    EmitID = testId,
                    Name = "Test " + testId.ToString(),
                    Telephone = new List<String>() { "95116354", "41530965", "48021455" },
                    Classes = new List<ParticipantClass>() { race.Classes.Find(x => x.Id.Equals("TEST")) },
                    IsTeam = true,
                    TeamMembers = new List<String>() { "Rune Kjørlaug", "Petter Stenstavold", "Erlend Klakegg Bergheim", "Even Østvold" },
                    ShirtSizes = new List<string>() { "m", "m", "m", "m" },
                    IsSuper = false,
                    IsCompany = false,
                    Ages = new List<int>() { 40, 40, 40, 40}
                };

                race.AddOrUpdateParticipant(parTest);
            }

            race.Testers = Testers;

            //Check for duplicated chips
            List<string> duplicated = race.Participants.GroupBy(x => x.EmitID)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key.ToString())
              .ToList<string>();

            if (duplicated.Count > 0)
                throw new Exception("Duplicate chipnumber: " + String.Join(", ", duplicated.ToArray()));

            // Create JSON dump and store in blob
            string json = JsonConvert.SerializeObject(race.Participants, Formatting.None);
            //blockBlob.UploadTextAsync(json).Wait();

            //// Persist on local disk
            //File.WriteAllText(@"c:\temp\data.json", json);

            DateTime start = new DateTime(_raceDate.Year, _raceDate.Month, _raceDate.Day, 13, 14, 0);

            foreach (Participant p in race.Participants)
            {
                Participant par = race.AddPass(new EmitData()
                {
                    BoxId = 1,
                    Id = p.EmitID,
                    Time = start
                });
            }
        }

        private string PrettyPrintName(string name)
        {
            String[] parts = name.Split(' ');
            parts = parts.Where(x => !string.IsNullOrEmpty(x)).ToArray<String>();

            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Substring(0, 1).ToUpper() + parts[i].Substring(1).ToLower();

            return String.Join(" ", parts);
        }

        private String CleanMobile(String n)
        {
            if (String.IsNullOrEmpty(n))
                return n;

            n = n.Replace(" ", "");
            //n = n.Replace("+47", "");

            if (!(n.Length == 0 || n.Length == 8))
                throw new Exception(n);

            return n;
        }

        private dynamic CallRestService(string uri, string method)
        {
            dynamic result;

            var req = HttpWebRequest.Create(uri);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            req.Method = method;
            req.ContentType = "application/json";

            using (var resp = req.GetResponse())
            {
                var results = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                result = JArray.Parse(results);
            }

            return result;
        }
    }
}
