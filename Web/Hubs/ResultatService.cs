using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

using EmitReaderLib;
using KjeringiData;
using MySql.Data.MySqlClient;

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
            if (String.IsNullOrEmpty(name))
            {
                foreach(Plassering p in Konkurranse.GetInstance.Plasseringar)
                    Groups.Add(Context.ConnectionId, p.Namn);
            }
            else
                Groups.Add(Context.ConnectionId, HttpContext.Current.Server.HtmlDecode(name));
        }

        public ICollection<SubSystem> GetConnectedSystems()
        {
            return _repository.Systems.ToList<SubSystem>();
        }

        public ICollection<ResultatData> GetPlassering(String name)
        {
            Plassering plassering = Konkurranse.GetInstance.Plasseringar.Find(x => x.Namn.Equals(HttpContext.Current.Server.HtmlDecode(name)));
            return plassering.Resultat.Take(100).Reverse().ToList<ResultatData>();
        }

        public void SendPassering(EmitData data)
        {
            // Persist
            MySqlConnection conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["kjeringi"].ConnectionString);
            conn.Open();

            MySqlCommand cmd = new MySqlCommand(@"insert into timers_raw (year, card, time, location) values (14, " + data.Id.ToString() + ", '" + data.Time.ToString("HH:mm:ss.FFF") + "', " + data.BoxId.ToString() + ");", conn);
            cmd.ExecuteNonQuery();
            conn.Close();

            // Send to Erlend
            SubmitWorker submitter = new SubmitWorker("http://ko.hoo9.com/timer/register");
            submitter.ProcessData(data);

            try
            {
                // Add new Passering to race
                ResultatData resultat = Konkurranse.GetInstance.RegistrerPassering(data);

                if (resultat != null)
                {
                    Clients.All.addLogMessage(resultat.ResultatForEtappe, resultat.EmitID, resultat.Startnummer, resultat.Namn, data.Time.ToLongTimeString());
                    Clients.Group(resultat.ResultatForEtappe).processResultat(resultat);
                }
            }
            catch (Exception ex) { }
        }
    }
}