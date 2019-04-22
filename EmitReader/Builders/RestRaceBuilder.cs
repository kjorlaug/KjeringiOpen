using EmitReaderLib.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EmitReaderLib.Builders
{
    public class RestRaceBuilder : JsonRaceBuilder
    {

        public RestRaceBuilder(String connection, List<int> testers, String year, DateTime raceDate) : base(connection, testers, year, raceDate) { }

        internal override dynamic GetJson(string uri)
        {
            dynamic result;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

            var req = HttpWebRequest.Create(uri);

            req.Method = "GET";
            req.ContentType = "application/json";

            using (var resp = req.GetResponse())
            {
                var results = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                result = JArray.Parse(results);
            }

            return result;
        }
    }
}
