using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EmitReaderLib;
using System.IO.Ports;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = ConfigurationManager.ConnectionStrings["Kjeringi"].ConnectionString;
            t += t + "Database=timers;";

            EmitMonitor monitor = new EmitMonitor(
                new MySqlTestReader(
                    100, t,
                    @"select 
	                    Location, Card, Time, case when timediff(Time, prevTime) < 0 then Time else timediff(Time,prevTime) end as diffTime
                    from (
                    SELECT 
	                    Location, 
                        Card-700 as Card, 
                        Time, 
                        (select max(Time) as prevTime from timers.locationpasses where year = 2018 and Time < p.Time) as prevTime
                    FROM timers.locationpasses p
                    where 
	                    year = 2018 and Time > '2018-04-14 13:14:00' order by Time) t;", new DateTime(2019, 4, 27)
                    ), 
                new SignalWorker()
                {
                    BoxId = "99",
                    Name = "Test 2019",
                    Hub = ConfigurationManager.AppSettings["hub"]
                }); // Testers

            monitor.StartMonitoring();

            Console.ReadLine();
        }
    }
}
