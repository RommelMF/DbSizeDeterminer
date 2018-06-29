using System;
using System.Collections.Generic;
using Npgsql;

namespace DbSizeDeterminer
{
    // class for connection in DB
    public class Connection
    {
        private string conn_param;
        
        // get connection with data
        public Dictionary<string, string> GetConnection()
        {
            string sql = "SELECT pg_database.datname as database_name, pg_database_size(pg_database.datname) as size FROM pg_database ORDER by pg_database_size(pg_database.datname) DESC;";
            
            NpgsqlConnection conn = new NpgsqlConnection(conn_param);
            NpgsqlCommand comm = new NpgsqlCommand(sql, conn);
            conn.Open();
            
            NpgsqlDataReader dataReader;
            dataReader = comm.ExecuteReader();
            Dictionary<string, string> dict = new Dictionary<string, string>();

            while (dataReader.Read())
            {
                try
                {
                    string db_name = dataReader.GetString(0);
                    string db_size = dataReader.GetInt32(1).ToString();
                    dict.Add(db_name, db_size);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            conn.Close();
            return dict;
        }

        public Connection(string conn_configuration)
        {
            this.conn_param = conn_configuration;
        }
    }
}