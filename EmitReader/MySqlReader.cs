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
        protected long lastCount;
        protected long lastId;
        protected Boolean stopSignal = false;

        public MySqlReader()
        {
            conn = new MySqlConnection("SERVER=localhost;database=kjeringi;uid=kjeringi;pwd=kjeringi;allow user variables=true;");
            lastCount = 0;
            lastId = 0;
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql_count, conn);
                    long count = (long)cmd.ExecuteScalar();

                    if (count > lastCount)
                    {
                        lastCount++;

                        // Get new rows
                        cmd = new MySqlCommand(sql_newrows, conn);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@id", lastId);

                        MySqlDataReader r = cmd.ExecuteReader();
                        r.Read();
                        lastId = (int)r[0];
                        r.Close();

                        MySqlCommand dataCmd = new MySqlCommand(String.Format(sql_data, lastId, lastId), conn);
                        MySqlDataReader data = dataCmd.ExecuteReader();
                        if (!data.Read())
                            throw new Exception("No data?");
                        data.NextResult();
                        data.Read();

                        EmitData d = new EmitData();
                        d.Id = (int)data["ident"];
                        //d.Time = data.GetTimeSpan("time_time");
                        d.Position = (long)data["pos"];
                        d.FirstName = (String)data["firstName"];
                        d.LastName = (String)data["surname"];
                        d.TeamName = (String)data["teamName"];
                        d.ClassCode = (String)data["classCode"];
                        d.Leg = (int)data["leg"];
                        d.LegTime = data.GetTimeSpan("time_diff");
                        d.TotalTime = data.GetTimeSpan("time_time");

                        EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                        if (handler != null)
                            handler(this, new EmitDataRecievedEventArgs(d));

                        data.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to read database:" + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }).ContinueWith((t) =>
            {
                if (!stopSignal)
                {
                    System.Threading.Thread.Sleep(1000);
                    Start();
                }
            });
        }

        public void Stop()
        {
            stopSignal = true;
        }

        protected String sql_count = @"SELECT count(id) FROM 2012_timers";
        protected String sql_newrows = @"SELECT id FROM (SELECT id FROM 2012_timers WHERE id > @id AND active = 1 order by id) t LIMIT 1";

        protected String sql_data = @"
            set @rownum := 0;
            select @classCode := classCode, @leg := leg from (
	            select 
		            t.id, t.ident, p.id as personId, p.firstName, p.surname, 'Superkjerring' as teamName, p.personClassCode as classCode, t.time_time, t.leg
	            from 
		            2012_timers t inner join 2012_person p on t.ident = p.cardid
	            union all
	            select
		            t.id, t.ident, team.id as personId, p.firstName, p.surname, team.name as teamName, team.teamClassCode as classCode, t.time_time, t.leg
	            from
		            (2012_timers t inner join 2012_team team on t.ident = team.cardid) inner join 2012_person p on (p.teamId = team.id and p.sprintNumber = t.leg)
            ) t where t.id = {0};
            select classTotals.*, ifnull(timediff(classTotals.time_time, (select min(time_time) from 2012_timers where ident = classTotals.ident and leg = classTotals.leg - 1)), classTotals.time_time) as time_diff from (
	            select @rownum := @rownum + 1 as pos, totals.* from (
		            select 
			            t.id, t.ident, p.id as personId, p.firstName, p.surname, 'Superkjerring' as teamName, p.personClassCode as classCode, t.time_time, t.leg
		            from 
			            2012_timers t inner join 2012_person p on t.ident = p.cardid
		            union all
		            select
			            t.id, t.ident, team.id as personId, p.firstName, p.surname, team.name as teamName, team.teamClassCode as classCode, t.time_time, t.leg
		            from
			            (2012_timers t inner join 2012_team team on t.ident = team.cardid) inner join 2012_person p on (p.teamId = team.id and p.sprintNumber = t.leg)
	            ) as totals
	            where
		            totals.classCode = @classCode and leg = @leg
	            order by totals.time_time
            ) as classTotals
            where classTotals.id = {1};";
    }
}
