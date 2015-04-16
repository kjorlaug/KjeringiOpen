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

        public MySqlWriter(String connectionName, String year)
        {
            ConnectionName = connectionName;
            Year = int.Parse(year);
        }

        protected String ConnectionName { get; set; }
        protected int Year { get; set; }

        public void PersistPass(Model.EmitData pass)
        {
            MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString);
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
