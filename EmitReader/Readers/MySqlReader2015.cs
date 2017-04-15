using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using MySql.Data.MySqlClient;

namespace EmitReaderLib
{
    public class MySqlReader2015 : IEmitReader
    {
        public event EventHandler<EmitDataRecievedEventArgs> DataReceived;

        public MySqlReader2015(int tempo, String connectionStringName, String year)
        {
            Tempo = tempo;
            ConnectionStringName = connectionStringName;
            Year = year;
        }

        protected int Tempo { get; set; }
        protected String ConnectionStringName { get; set; }
        protected String Year { get; set; }

        public void Start()
        {
            MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM History WHERE year = " + Year + " ORDER BY time", conn);

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
                        EmitData d = new EmitData()
                        {
                            Id = data.GetInt16("card"),
                            BoxId = data.GetInt16("location"),
                            Time = data.GetDateTime("time"),
                            Force = false
                        };

                        EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                        if (handler != null)
                            handler(this, new EmitDataRecievedEventArgs(d));

                        System.Threading.Thread.Sleep(50);
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
