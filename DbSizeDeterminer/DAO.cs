using System.Collections.Generic;
using System.Configuration;

namespace DbSizeDeterminer
{
    // class for access data
    public class DAO
    {   
        //get data from config servers
        public List<Server> GetConfigData()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            List<Server> servers = new List<Server>();

            for (int i = 1; i < ConfigurationManager.AppSettings.Count + 1; i++)
            {
                var key = ConfigurationManager.AppSettings.GetKey(i - 1);
                var value = ConfigurationManager.AppSettings[key];
                dict.Add(key, value);
                if(i % 3 == 0) {
                    servers.Add(new Server(dict["name"], dict["dbConfig"], dict["size"]));
                    dict.Clear();
                }
            }          
            return servers;
        }
        //get data from database
        public void GetDbData(List<Server> servers)
        {
            foreach (var server in servers)
            {
                Connection conn = new Connection(server.dbConfig);
            
                server.dbData = conn.GetConnection();
            }     
        }   
    }
}