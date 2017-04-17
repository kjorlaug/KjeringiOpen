using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using EmitReaderLib;
using EmitReaderLib.Model;
using KjeringiData;

namespace Web.Controllers
{
    public class RaceController : ApiController
    {

        // GET api/<controller>
        public IEnumerable<int> Get()
        {
            return new List<int>() { 2013, 2014, 2015, 2016, 2017};
        }

        public IEnumerable<ParticipantClass> Get(int year)
        {
            return TheRace.Historical(year).Classes.OrderBy(p => p.Sequence);            
        }

        // GET api/<controller>/5
        public ICollection<Participant> Get(int year, String className)
        {
            try {
                return TheRace.Historical(year).GetResults(className);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}