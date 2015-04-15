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
        public IEnumerable<ParticipantClass> Get()
        {
            return TheRace.Instance.Classes;            
        }

        // GET api/<controller>/5
        public ICollection<Participant> Get(String id)
        {
            return TheRace.Instance.GetResults(id);
        }

    }
}