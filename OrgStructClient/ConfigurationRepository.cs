using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgStructClient
{
    public static class ConfigurationRepository
    {
        static ConfigurationRepository()
        {
            ServerURL = @"http://localhost:9000/";
        }

        public static string ServerURL { set; get; }

    }
}
