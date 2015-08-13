using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowConnector
{
    public class SNConfig
    {
        public string username { get; set; }
        public string password { get; set; }
        public string org { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string logLocation { get; set; }

        private static volatile SNConfig instance;
        private static object syncRoot = new Object();

        private SNConfig()
        {
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
            org = ConfigurationManager.AppSettings["org"];
            client_id = ConfigurationManager.AppSettings["client_id"];
            client_secret = ConfigurationManager.AppSettings["client_secret"];
            logLocation = ConfigurationManager.AppSettings["logLocation"];
        }

        public static SNConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SNConfig();
                    }
                }

                return instance;
            }
        }
    }
}
