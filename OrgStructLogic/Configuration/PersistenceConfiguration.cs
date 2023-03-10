using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgStructLogic.Configuration
{
    public class PersistenceConfiguration
    {
        public PersistenceConfiguration()
        {
            // defaults
            DefaultOrganizationName = "Default Organization";
            DataSource = "Default Organization.json";
            SyncDelayMilliseconds = 500;
            SyncMaxDeferredChanges = 20;
        }

        public string DefaultOrganizationName { set; get; }

        public string DataSource { set; get; }

        public int SyncDelayMilliseconds { set; get; }

        public int SyncMaxDeferredChanges { set; get; }
    }
}
