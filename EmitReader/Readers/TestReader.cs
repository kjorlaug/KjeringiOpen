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

        public TestReader(List<int> testers)
        {
            Testers = testers;
        }

        protected List<int> Testers { get; set; }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach(int test in Testers)
                    {
                        DateTime time = DateTime.Now;
                        EmitData d = new EmitData()
                            {
                                Id = test,
                                BoxId = 248,
                                Time = DateTime.Now,
                                Force = false
                            };

                            EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                            if (handler != null)
                                handler(this, new EmitDataRecievedEventArgs(d));

                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public void Stop()
        {
        }
    }
}
