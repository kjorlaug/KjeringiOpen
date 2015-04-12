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

        #region IDisconnect and IConnected event handlers implementation

        /// <summary>
        /// Fired when a client disconnects from the system. The user associated with the client ID gets deleted from the list of currently connected users.
        /// </summary>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
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

            return base.OnDisconnected(stopCalled);
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
                foreach(TimeStation ts in TheRace.Instance.TimeStations)
                    Groups.Add(Context.ConnectionId, ts.Name);
            }
            else
                Groups.Add(Context.ConnectionId, HttpContext.Current.Server.HtmlDecode(name));
        }

        public ICollection<SubSystem> GetConnectedSystems()
        {
            return _repository.Systems.ToList<SubSystem>();
        }

        public ICollection<Result> GetPlassering(String name)
        {
            //Plassering plassering = Konkurranse.GetInstance.Plasseringar.Find(x => x.Namn.Equals(HttpContext.Current.Server.HtmlDecode(name)));
            //return plassering.Resultat.Take(100).Reverse().ToList<ResultatData>();
            return null;
        }

        public void SendPassering(EmitData data)
        {
            //// Persist
            //MySqlConnection conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["kjeringi"].ConnectionString);
            //conn.Open();

            //MySqlCommand cmd = new MySqlCommand(@"insert into timers_raw (year, card, time, location) values (14, " + data.Id.ToString() + ", '" + data.Time.ToString("HH:mm:ss.FFF") + "', " + data.BoxId.ToString() + ");", conn);
            //cmd.ExecuteNonQuery();
            //conn.Close();

            //// Send to Erlend
            //SubmitWorker submitter = new SubmitWorker("http://ko.hoo9.com/timer/register");
            //submitter.ProcessData(data);

            try
            {
                // Add new Passering to race
                Result resultat = TheRace.Instance.AddPass(data);

                if (resultat != null)
                {
                    Clients.All.addLogMessage(resultat.CurrentSplit, resultat.EmitID, resultat.Startnumber, resultat.Name, data.Time.ToLongTimeString());
                    Clients.Group(resultat.StationName).processResultat(resultat);
                }
            }
            catch (Exception ex) { }
        }
    }
}