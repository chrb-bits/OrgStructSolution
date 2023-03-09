using System;

namespace OrgStructLogic.Configuration
{
    public class ConfigurationRepository
    {
        public string ServerURL { get { return @"http://localhost:9000/"; } }

        public string PersistenceDataSource { get { return "Default Organization.json";  } }

        public string OrganizationName { get { return "Default Organization"; } }

        public int PersistenceFlushDelayMilliseconds {  get { return 2000; } }

        public int PersistenceSyncMaxDeferredChanges { get { return 20; } }

        public TimeSpan LockTimeout { get { return new TimeSpan(0, 0, 60); } }
    }
}
