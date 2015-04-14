using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using System.Data.SqlClient;

namespace EmitReaderLib
{
    public class SqlReader : IEmitReader
    {
        public event EventHandler<EmitDataRecievedEventArgs> DataReceived;

        protected int tempo = 120;

        public void Start()
        {
            SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["kjeringi"].ConnectionString);
            SqlCommand cmd = new SqlCommand("SELECT * FROM timers_raw WHERE location > 0 ORDER BY id", conn);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    conn.Open();
                    SqlDataReader data = cmd.ExecuteReader();

                    DateTime nextSleep = DateTime.Now;
                    Boolean init = false;

                    while (data.Read())
                    {
                        DateTime time = DateTime.ParseExact(data["time"].ToString(), "HH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
                        if (!init)
                        {
                            nextSleep = time.AddSeconds(tempo);
                            init = true;
                        }

                        if (time < nextSleep)
                        {
                            EmitData d = new EmitData()
                            {
                                Id = int.Parse(data["card"].ToString()),
                                BoxId = int.Parse(data["location"].ToString()),
                                Time = time
                            };
                            EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                            if (handler != null)
                                handler(this, new EmitDataRecievedEventArgs(d));

                        }
                        else
                        {
                            System.Threading.Thread.Sleep(1000);
                            nextSleep = time.AddSeconds(tempo);
                        }
                    }
                    data.Close();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to read database:" + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public void Stop()
        {
        }
    }
}
