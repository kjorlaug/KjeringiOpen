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
            if (!TheRace.Instance.InTestMode)
                (new EmitReaderLib.Writers.MySqlWriter("kjeringi.writer", TheRace.Instance.Name)).PersistPass(data);

//            try
  //          {
                // Tester?
                data.Test = TheRace.Instance.Testers.Contains(data.Id);

                // Add new Passering to race
                Participant resultat = TheRace.Instance.AddPass(data);
                var timestation = TheRace.Instance.TimeStations.First(ts => ts.Id.Equals(data.BoxId));

                if (resultat != null)
                {
                    Clients.All.addLogMessage(resultat.Splits().Count > 0 ? resultat.Splits().Last().Time : "", resultat.EmitID, resultat.Startnumber, resultat.Name, resultat.EstimatedTime);
                    Clients.Group(timestation.Id.ToString()).newPass(resultat);
                }
                else
                {
                    Clients.All.addLogMessage("No result generated", data.Id, data.BoxId, data.Time, "");
                }

            //List<RaceEvent> events = TheRace.Instance.AnalyzePass(data);


   //         }
   //         catch (Exception ex) {
   //             Clients.All.addLogMessage("Exception: " + ex.Message, data.Id, data.BoxId, data.Time, "");
   //         }
        }

        public ICollection<Participant> GetCurrentResults(String participantClassId)
        {
            return TheRace.Instance.GetResults(participantClassId);
        }

        public ICollection<Participant> GetExpected()
        {
            return TheRace.Instance.Participants.Where(p => !p.Finished && p.Passes.Count > 1).OrderByDescending(p => p.EstimatedArrival).Take(20).ToList<Participant>();
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
                    .Where(p => p.Passes.ContainsKey(ts.Id))
                    //.SelectMany(p => p.Splits().Where(s => s.Location == ts.Id))
                    .OrderByDescending(t => t.CurrentTime)
                    .ToList<Participant>()
                    //.OrderByDescending(t => t._splits.OrderBy(s=>s.Ticks).Last().Ticks)
                    .Take(100);
            }
        }


    }
}