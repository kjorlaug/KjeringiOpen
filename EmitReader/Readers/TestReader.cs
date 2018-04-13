using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using MySql.Data.MySqlClient;

namespace EmitReaderLib
{
    public class TestReader : IEmitReader
    {
        public event EventHandler<EmitDataRecievedEventArgs> DataReceived;
        public String Port { get; set; }
        public String BoxId { get; set; }

        public TestReader(List<int> testers, List<int> stations)
        {
            Testers = testers;
            Stations = stations;
        }

        protected List<int> Testers { get; set; }
        protected List<int> Stations { get; set; }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Dictionary<int, DateTime> timers = new Dictionary<int, DateTime>();
                    Random rnd = new Random();
                    
                    // init start
                    foreach(int test in Testers)
                        timers.Add(test, new DateTime(2018,4,14,13,14,00));


                    foreach (int box in Stations) { 
                        foreach(int test in Testers) {
                            // Leap forward 10-20 min
                            timers[test] = timers[test].AddSeconds(rnd.Next(600, 1200));
                            
                            EmitData d = new EmitData()
                                {
                                    Id = test,
                                    BoxId = box,
                                    Time = timers[test],
                                    Force = false
                                };

                                EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                                if (handler != null && int.Parse(BoxId) == box)
                                    handler(this, new EmitDataRecievedEventArgs(d));

                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }).Wait();
        }

        public void Stop()
        {
        }
    }
}
