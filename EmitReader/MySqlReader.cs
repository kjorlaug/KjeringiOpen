using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace EmitReaderLib
{
    public class MySqlReader : IEmitReader
    {
        public event EventHandler<EmitDataRecievedEventArgs> DataReceived;

        protected MySqlConnection conn;
        protected MySqlCommand cmd;
        protected int tempo = 10;

        public MySqlReader()
        {
            conn = new MySqlConnection("SERVER=localhost;database=kop2014;uid=root;pwd=difi;allow user variables=true;");

            cmd = new MySqlCommand(@"select 
	                distinct cardid, time, location
                from (
	                select 
		                card + 1000 as cardId,
		                time,
		                case 
			                when location = 78 then 66
			                when location = 70 then 70
			                when location = 79 then 72
			                else location
		                end as location
	                from
		                timers_raw
	                where
		                location <> 75
	                ) as t,
	                (
	                select
		                s.id, chips.chipNumber
	                from 
		                kop_station s,
		                (
			                select chipNumber from kop_person where superwife = 1 and deleted = 0
			                union 
			                select chipNumber from kop_team where deleted = 0
		                ) as chips
	                where
		                s.official = 1
	                ) as d
                where
	                d.chipNumber = t.cardid
                and t.cardid <> 6107
                order by 2", conn);
        }

        public void Start()
        {
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

                        DateTime time = DateTime.ParseExact(data.GetString("time"), "HH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
                        if (!init)
                        {
                            nextSleep = time.AddSeconds(tempo);
                            init = true;
                        }

                        if (time < nextSleep)
                        {
                            EmitData d = new EmitData()
                            {
                                Id = int.Parse(data.GetString("cardid")),
                                BoxId = int.Parse(data.GetString("location")),
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
