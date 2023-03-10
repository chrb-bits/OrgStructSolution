using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgStructLogic.Configuration
{
    public class ServiceConfiguration
    {
        public ServiceConfiguration()
        {
            // defaults
            ServiceURL = @"http://localhost:9000/";
            ObjectLockTimeout = new TimeSpan(0, 0, 60);
        }

        public string ServiceURL { set; get; }

        public TimeSpan ObjectLockTimeout { set; get; }
    }
}
