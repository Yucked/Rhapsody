using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Configuration
{
    public class Configuration
    {
        public string Host { get; set; }

        public short Port { get; set; }

        public string Authorization { get; set; }

        public Configuration(string host, short port, string authorization)
        {
            Host = host;
            Port = port;
            Authorization = authorization;
        }

        public Configuration()
        {
        }
    }
}