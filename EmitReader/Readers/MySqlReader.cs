using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using MySql.Data.MySqlClient;

namespace EmitReaderLib
{
    public class MySqlReader : IEmitReader
    {
        public event EventHandler<EmitDataRecievedEventArgs> DataReceived;

        public MySqlReader(int tempo, String connectionStringName, List<String> locations, String year, String cardStart, String cardEnd)
        {
            Tempo = tempo;
            ConnectionStringName = connectionStringName;
            Locations = locations;
            Year = year;
            CardStart = cardStart;
            CardEnd = cardEnd;
        }

        protected int Tempo { get; set; }
        protected String ConnectionStringName { get; set; }
        protected List<String> Locations { get; set; }
        protected String Year { get; set; }
        protected String CardStart { get; set; }
        protected String CardEnd { get; set; }

        public void Start()
        {
            MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM timers_raw WHERE year = " + Year + " AND location in (" + String.Join(",", Locations) + ") and card between " + CardStart + " AND " + CardEnd + " ORDER BY id", conn);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    conn.Open();
                    MySqlDataReader data = cmd.ExecuteReader();

                    DateTime nextSleep = DateTime.Now;
                    Boolean init = false;

                    while (data.Read())
                    {
                        DateTime time;
                        if (DateTime.TryParseExact(data["time"].ToString(), "HH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out time))
                        {
                            EmitData d = new EmitData()
                                {
                                    Id = int.Parse(data["card"].ToString()),
                                    BoxId = int.Parse(data["location"].ToString()),
                                    Time = time,
                                    Force = false
                                };

                            EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                            if (handler != null)
                                handler(this, new EmitDataRecievedEventArgs(d));
                        }
                        else
                            Console.WriteLine("Unable to parse: " + data["time"].ToString());

                        System.Threading.Thread.Sleep(300);
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
