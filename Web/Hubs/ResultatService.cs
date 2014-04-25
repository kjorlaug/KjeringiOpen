using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

using EmitReaderLib;
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

        #region IDisconnect and IConnected event handlers implementation

        /// <summary>
        /// Fired when a client disconnects from the system. The user associated with the client ID gets deleted from the list of currently connected users.
        /// </summary>
        /// <returns></returns>
        public override Task OnDisconnected()
        {
            string sysId = _repository.GetSystemByConnectionId(Context.ConnectionId);
            if (sysId != null)
            {
                SubSystem sys = _repository.Systems.Where(u => u.Id == sysId).FirstOrDefault();
                if (sys != null)
                {
                    _repository.Remove(sys);
                    return Clients.All.leaves(sys.Id, sys.SystemName, DateTime.Now.ToShortTimeString());
                }
            }

            return base.OnDisconnected();
        }

        #endregion

        public void Join(String name)
        {
            SubSystem sys = new SubSystem()
            {
                //Id = Context.ConnectionId,                
                Id = Guid.NewGuid().ToString(),
                SystemName = name, 
                TimeStamp = DateTime.Now.ToShortTimeString()
            };
            _repository.Add(sys);
            _repository.AddMapping(Context.ConnectionId, sys.Id);
            Clients.All.joins(sys.Id, sys.SystemName, sys.TimeStamp);
        }

        public void AddtoGroup(String name)
        {
            Groups.Add(Context.ConnectionId, name);
        }

        public ICollection<SubSystem> GetConnectedSystems()
        {
            return _repository.Systems.ToList<SubSystem>();
        }

        public void SendPassering(EmitData data)
        {
            // Add new Passering to race
            ResultatData resultat = Konkurranse.GetInstance.RegistrerPassering(data);

            if (resultat != null)
            {
                Clients.All.addLogMessage(resultat.ResultatForEtappe, resultat.EmitID, resultat.Startnummer, resultat.Namn, data.Time.ToLongTimeString());
                Clients.Group(resultat.ResultatForEtappe).processResultat(resultat);
            }
        }
    }
}