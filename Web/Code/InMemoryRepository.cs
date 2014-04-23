using System;
using System.Collections.Generic;
using System.Linq;

namespace KjeringiData
{
    public class InMemoryRepository
    {
        private static ICollection<SubSystem> _connectedSystems;
        private static Dictionary<string, string> _mappings;
        private static InMemoryRepository _instance = null;
        private static readonly int max_random = 3;
        
        public static InMemoryRepository GetInstance()
        {
            if (_instance == null)
            {
                _instance = new InMemoryRepository();                
            }
            return _instance;
        }

        #region Private methods

        private InMemoryRepository()
        {
            _connectedSystems = new List<SubSystem>();
            _mappings = new Dictionary<string, string>();
        }

        #endregion

        #region Repository methods

        public IQueryable<SubSystem> Systems { get { return _connectedSystems.AsQueryable(); } }

        public void Add(SubSystem sys)
        {
            _connectedSystems.Add(sys);
        }

        public void Remove(SubSystem sys)
        {
            _connectedSystems.Remove(sys);
        }

        public string GetRandomizedName(string name)
        {
            string tempName = name;
            int newRandom = max_random, oldRandom = 0;
            int loops = 0;
            Random random = new Random();
            do
            {
                if (loops > newRandom)
                {
                    oldRandom = newRandom;
                    newRandom *= 2;
                }
                name = tempName + "_" + random.Next(oldRandom, newRandom).ToString();
                loops++;
            } while (GetInstance().Systems.Where(u => u.SystemName.Equals(name)).ToList().Count > 0);

            return name;
        }

        public void AddMapping(string connectionId, string userId)
        {
            if (!string.IsNullOrEmpty(connectionId) && !string.IsNullOrEmpty(userId))
            {
                _mappings.Add(connectionId, userId);
            }
        }

        public string GetSystemByConnectionId(string connectionId)
        {
            string sysId = null;
            _mappings.TryGetValue(connectionId, out sysId);            
            return sysId;
        }

        #endregion
    }
}