using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using MySql.Data.MySqlClient;

namespace EmitReaderLib
{
    public class MySqlTestReader : IEmitReader
    {
        public event EventHandler<EmitDataRecievedEventArgs> DataReceived;
        public String Port { get; set; }
        public String BoxId { get; set; }
        
        public MySqlTestReader(int tempo, String connectionString, String sql, DateTime raceDate)
        {
            Tempo = tempo;
            ConnectionString = connectionString;
            Sql = sql;
            RaceDate = raceDate;
        }

        protected int Tempo { get; set; }
        protected String ConnectionString { get; set; }
        protected String Sql { get; set; }
        protected DateTime RaceDate { get; set; }

        public void Start()
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = new MySqlCommand(Sql, conn);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    conn.Open();
                    MySqlDataReader data = cmd.ExecuteReader();

                    bool isFirst = true;

                    while (data.Read())
                    {
                        DateTime time = data.GetDateTime("Time");
                        DateTime newTime = new DateTime(RaceDate.Year, RaceDate.Month, RaceDate.Day, time.Hour, time.Minute, time.Second);

                        EmitData d = new EmitData()
                        {
                            Id = data.GetInt16("Card"),
                            BoxId = data.GetInt16("Location"),
                            Time = newTime,
                            Force = false
                        };

                        EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                        if (handler != null)
                            handler(this, new EmitDataRecievedEventArgs(d));

                        DateTime diff = data.GetDateTime("diffTime");
                        int milliseconds = diff.TimeOfDay.Seconds * 1000 + diff.TimeOfDay.Milliseconds;
                        if (milliseconds > 0 && !isFirst)
                            System.Threading.Thread.Sleep(milliseconds / Tempo);
                        isFirst = false;
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
