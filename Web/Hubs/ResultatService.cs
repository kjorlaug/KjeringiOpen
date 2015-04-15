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
                    Groups.Add(Context.ConnectionId, ts.Name);
            }
            else
                Groups.Add(Context.ConnectionId, name);
        }

        public void SendPassering(EmitData data)
        {
            //try
            //{
                // Add new Passering to race
                Participant resultat = TheRace.Instance.AddPass(data);
                var timestation = TheRace.Instance.TimeStations.First(ts => ts.Id.Equals(data.BoxId));

                if (resultat != null)
                {
                    Clients.All.addLogMessage( /*resultat.CurrentSplit */ "", resultat.EmitID, resultat.Startnumber, resultat.Name, data.Time.ToLongTimeString());
                    Clients.Group(timestation.Name).newPass(resultat);
                }
            //}
            //catch (Exception ex) { 
            //    // Duplicate
            //}
        }

        public ICollection<Participant> GetCurrentResults(String participantClassId)
        {
            return TheRace.Instance.GetResults(participantClassId);
        }

        public ICollection<Result> GetPlassering(String name)
        {
            //Plassering plassering = Konkurranse.GetInstance.Plasseringar.Find(x => x.Namn.Equals(HttpContext.Current.Server.HtmlDecode(name)));
            //return plassering.Resultat.Take(100).Reverse().ToList<ResultatData>();
            return null;
        }


    }
}