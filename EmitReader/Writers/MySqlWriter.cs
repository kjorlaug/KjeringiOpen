using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;
using MySql.Data.MySqlClient;

namespace EmitReaderLib.Writers
{
    public class MySqlWriter : IRaceWriter
    {

        public MySqlWriter(String connectionName)
        {
            // Create correct connection string
            String t = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");

            if (!String.IsNullOrEmpty(t))
            {
                t = t.Substring(t.IndexOf(":") + 1);
                t = t.Substring(0, t.IndexOf(";"));

                t = "Server=127.0.0.1;Port=" + t + ";Uid = azure; Pwd = 6#vWHD_$;Database=timers;";
            }
            else
                t = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;


            this.ConnectionName = t;
            this.Year = DateTime.Now.Year;
        }

        protected String ConnectionName { get; set; }
        protected int Year { get; set; }

        public void PersistPass(Model.EmitData pass)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionName);
            conn.Open();

            MySqlCommand cmd = new MySqlCommand("INSERT INTO LocationPasses (`Year`, `Location`, `Card`, `Time`, `Force`, `Test`) VALUES (@year, @location, @card, @time, @force, @test)", conn);

            cmd.Prepare();
            cmd.Parameters.AddWithValue("@year", Year);
            cmd.Parameters.AddWithValue("@location", pass.BoxId);
            cmd.Parameters.AddWithValue("@card", pass.Id);
            cmd.Parameters.AddWithValue("@time", pass.Time);
            cmd.Parameters.AddWithValue("@force", pass.Force);
            cmd.Parameters.AddWithValue("@test", pass.Test);

            cmd.ExecuteNonQuery();

            conn.Close();
        }
    }
}
