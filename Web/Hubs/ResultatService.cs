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
                    return Clients.All.leaves(sys.Id, sys.SystemName, DateTime.Now);
                }
            }

            return base.OnDisconnected();
        }

        #endregion

        public void Joined()
        {
            SubSystem sys = new SubSystem()
            {
                //Id = Context.ConnectionId,                
                Id = Guid.NewGuid().ToString(),
                SystemName = Clients.Caller.username
            };
            _repository.Add(sys);
            _repository.AddMapping(Context.ConnectionId, sys.Id);
            Clients.All.joins(sys.Id, Clients.Caller.username, DateTime.Now);
        }

        public ICollection<SubSystem> GetConnectedSystems()
        {
            return _repository.Systems.ToList<SubSystem>();
        }


        public void SendPassering(EmitData data)
        {
            //Clients.All.nyPassering(data.BoxId, data.Time);
            Clients.All.addNewMessageToPage(data.BoxId.ToString(), data.Time.ToLongTimeString());
        }

        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(name, message);
        }
    }
}