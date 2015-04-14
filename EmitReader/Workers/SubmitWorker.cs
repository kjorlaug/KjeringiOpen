using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using EmitReaderLib.Model;

namespace EmitReaderLib
{
    public class SubmitWorker : IWorker
    {

        protected TaskFactory factory;
        protected String baseUrl;

        public SubmitWorker(String url)
        {
            factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(5));
            baseUrl = url;
        }

        public void ProcessData(EmitData data)
        {
            factory.StartNew<bool>(() =>
                {
                    try
                    {
                        String url = String.Format(baseUrl + "?card={0}&time={1:HH:mm:ss.fff}&location={2}", data.Id, data.Time, data.BoxId);
                        WebClient webClient = new WebClient();
                        Stream stream = webClient.OpenRead(url);
                        StreamReader reader = new StreamReader(stream);
                        String request = reader.ReadToEnd();
                        Console.WriteLine("Success: " + url);
                    }
                    catch (WebException ex)
                    {
                        return false;
                    }
                    return true;
                }).ContinueWith((t) => {
                    if (!t.Result)
                    {
                        // failed to upload for some reason - put back in queue
                        Console.WriteLine("Failed - retry");
                        ProcessData(data);
                    }                           
                });
        }
    }
}
