using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

using EmitReaderLib;
using EmitReaderLib.Model;

using KjeringiData;
using System.Text;
using System.Net;
using System.IO;

namespace Web.Hubs
{
    public class ResultatServiceHub : Hub
    {
        private InMemoryRepository _repository;

        public ResultatServiceHub()
        {
            _repository = InMemoryRepository.GetInstance();
        }

        /// <summary>
        /// Fired when a client disconnects from the system. The user associated with the client ID gets deleted from the list of currently connected users.
        /// </summary>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            SubSystem sys = _repository.Systems.Where(u => u.Id == Context.ConnectionId).FirstOrDefault();
            if (sys != null)
            {
                _repository.Remove(sys);
                sys.Connected = false;
                Clients.All.SystemStatusChanged(sys);
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            SubSystem sys = _repository.Systems.Where(u => u.Id == Context.ConnectionId).FirstOrDefault();
            if (sys != null)
            {
                sys.TimeStamp = DateTime.Now.ToShortTimeString();
                sys.Connected = true;
                Clients.All.SystemStatusChanged(sys);
            }
            return base.OnReconnected();
        }
    
        public void Join(String name)
        {
            SubSystem sys = new SubSystem()
            {
                Id = Context.ConnectionId,
                SystemName = name, 
                TimeStamp = DateTime.Now.ToShortTimeString(),
                Connected = true
            };
            
            _repository.Add(sys);
            Clients.All.SystemStatusChanged(sys);
        }

        public ICollection<SubSystem> GetConnectedSystems()
        {
            return _repository.Systems.ToList<SubSystem>();
        }

        public void AddtoGroup(String name)
        {
            if (String.IsNullOrEmpty(name))
            {
                foreach(TimeStation ts in TheRace.Instance.TimeStations)
                    Groups.Add(Context.ConnectionId, ts.Id.ToString());
            }
            else
                Groups.Add(Context.ConnectionId, name);
        }

        public void SendPassering(EmitData data)
        {
            try
            {
                if (!TheRace.Instance.InTestMode)
                    (new EmitReaderLib.Writers.MySqlWriter("kjeringi.writer")).PersistPass(data);

                // Tester?
                data.Test = TheRace.Instance.Testers.Contains(data.Id);

                int[] leaders = GetRaceLeaders().Select(p => p.EmitID).ToArray<int>();

                // Add new Passering to race
                Participant resultat = TheRace.Instance.AddPass(data);
                var timestation = TheRace.Instance.TimeStations.First(ts => ts.Id.Equals(data.BoxId));

                if (resultat != null)
                {
                    Clients.All.addLogMessage(resultat.Splits().Count > 0 ? resultat.Splits().Last().Time : "", resultat.EmitID, resultat.Startnumber, resultat.Name, resultat.EstimatedTime);
                    Clients.Group(timestation.Id.ToString()).newPass(resultat);

                    // Change in Race leaders?
                    int[] newLeaders = GetRaceLeaders().Select(p => p.EmitID).ToArray<int>();

                    if (!leaders.Equals(newLeaders))
                        Clients.All.addLogMessage("NewRaceLead", resultat.EmitID, resultat.Startnumber, resultat.Name, resultat.EstimatedTime);

                    List<Result> res = resultat._splits.Where(r => r.Location.Equals(data.BoxId)).ToList<Result>();

                    foreach (Result topResult in res.Where(r => r.Position <= 3)) {
                        Clients.All.addLogMessage("NewTopResult " + topResult.Location + " " + topResult.Position.ToString() + ". " + topResult.Class + " " + topResult.Name, resultat.EmitID, resultat.Startnumber, resultat.Name, resultat.EstimatedTime);
                    }

                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        // Send SMS
                        StringBuilder sb = new StringBuilder();

                        sb.Append("Mellombels resultat Kjeringi Open 2019 %0A");
                        sb.Append(resultat.Name);
                        sb.Append(" (");
                        sb.Append(resultat.Classes[0].Name);
                        sb.Append(")%0AEtapper:%0A");

                        List<String[]> splits = resultat.Splits(resultat.Classes[0].Id).Select(p => new String[] { p.Leg, p.IsSuper ? "" : p.Name, (p.Estimated ? "(mangler)" : p.Time) }).ToList<String[]>();

                        foreach (Result r in resultat.Splits(resultat.Classes[0].Id))
                        {
                            sb.Append(" ");
                            sb.Append(r.Leg);
                            sb.Append(" ");
                            sb.Append(r.Estimated ? "(mangler)" : r.Time);
                            sb.Append(" (");
                            sb.Append(r.Position);
                            sb.Append(".plass) ");
                            sb.Append("%0A");
                        }
                        sb.Append("Totaltid: ");
                        sb.Append(resultat.TotalTime);

                        sb.Append("%0ASMS-tjenestene levert av Difi i samarbeid med Linkmobility");

                        foreach (String tlf in resultat.Telephone.Distinct())
                        {
                            try
                            {
                                String url = String.Format(@"http://simple.pswin.com/?USER=kjeringiopen&{0}&RCV=47{1}&TXT={2}&snd=Kjeringi&ENC=utf-8", "PW=0DgFPq2k3", tlf, sb.ToString());
                                WebClient webClient = new WebClient();
                                Stream stream = webClient.OpenRead(url);
                                StreamReader reader = new StreamReader(stream);
                                String request = reader.ReadToEnd();
                                Console.WriteLine("Success: " + url);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                    });
                }
                else
                {
                    Clients.All.addLogMessage("No result generated", data.Id, data.BoxId, data.Time, "");
                }

            }
            catch (Exception ex)
            {
                Clients.All.addLogMessage("Exception: " + ex.Message, data.Id, data.BoxId, data.Time, "");
            }
        }

        public ICollection<Participant> GetCurrentResults(String participantClassId)
        {
            return TheRace.Instance.GetResults(participantClassId);
        }

        public IEnumerable<Participant> GetRaceLeaders()
        {
            return TheRace.Instance.Participants
                .Where(p => !p.Finished && p.Passes.Count > 1)
                .OrderByDescending(t => t.Passes.Keys.Max()).ThenBy(t => t.CurrentTime.Ticks)
                .ToList<Participant>()
                .Take(10);
        }

        public IEnumerable<Participant> GetLatestLocationResult(String id)
        {
            TimeStation ts = TheRace.Instance.TimeStations.Find(t => t.Id.Equals(int.Parse(id)));

            if (id.Equals("93")) // Incoming :)
            {
                return TheRace.Instance.Participants
                    .Where(p => !p.Finished && p.Passes.ContainsKey(ts.Id))
                    .OrderByDescending(p => p.EstimatedArrival)
                    .Take(10).ToList<Participant>();
            }
            else
            {
                return TheRace.Instance.Participants
                    .Where(p => p.Passes.Count > 0 && p.Passes.ContainsKey(ts.Id))
                    .OrderByDescending(t => t._splits.Where(s => s.Location.Equals(ts.Id)).First().TicksSoFar)
                    .ToList<Participant>()
                    .Take(100);
            }
        }

    }
}