using System.Collections.Generic;

namespace DbSizeDeterminer
{
    // class model servers data
    public class Server
    {
        public string name { get;}
        public string dbConfig { get;}
        public string size { get;}
        public Dictionary<string, string> dbData { get; set; }

        public Server(string name, string dbConfig, string size)
        {
            this.name = name;
            this.dbConfig = dbConfig;
            this.size = size;
        }
    }
}