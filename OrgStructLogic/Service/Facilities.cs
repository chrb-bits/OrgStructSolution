using OrgStructLogic.Configuration;
using OrgStructLogic.ObjectManagement;
using OrgStructModels.Metadata;
using OrgStructModels.Persistence;
using OrgStructPersistence;
using System;
using System.IO;

namespace OrgStructLogic.Service
{
    public delegate void FacilitiesLogEventHandler(object sender, LogEventArgs e);

    public static class Facilities
    {
        #region Privates
        private static void Log(string message)
        {
            LogEvent?.Invoke(null, new LogEventArgs(message));
        }

        private static void SubsystemLogEvent(object sender, LogEventArgs e)
        {
            LogEvent?.Invoke(sender, e);
        }
        #endregion

        #region Public Interface
        public static event FacilitiesLogEventHandler LogEvent;

        public static ConfigurationRepository Configuration { private set; get; }

        public static IPersistence PersistenceLayer { private set; get; }
        
        public static ObjectLockManager ObjectLocks { private set; get; }

        public static UnUpdateableChangesTracker UnUpdateables { private set; get; }

        public static bool Running { private set; get; }
        

        public static void Start()
        {
            // setup configuration repository (currently completely static)
            Configuration = new ConfigurationRepository();
            if (Configuration != null)
            {
                Configuration.LogEvent += SubsystemLogEvent;

                Log("Configuration repository initialized.");

                // service configuration file exists?
                if (File.Exists("OrgStructLogic.Config.json"))
                {
                    // yes, load
                    Configuration.Load("OrgStructLogic.Config.json");
                }
                else
                {
                    // no, create default
                    Configuration.Save("OrgStructLogic.Config.json");
                }

                // setup persistence layer
                PersistenceLayer = new PersistenceLayer()
                {
                    DataSource = Configuration.Persistence.DataSource,
                    SyncDelayMilliseconds = Configuration.Persistence.SyncDelayMilliseconds,
                    SyncMaxDeferredChanges = Configuration.Persistence.SyncMaxDeferredChanges
                };

                if (PersistenceLayer != null)
                {
                    // configure default org name on persistence layer
                    PersistenceLayer.DefaultOrganizationName = Configuration.Persistence.DefaultOrganizationName;

                    // hook persistence layer log event
                    PersistenceLayer.LogEvent += SubsystemLogEvent;

                    Log("Persistence layer (" + PersistenceLayer.GetType().Assembly.GetName().Name + ") initialized.");

                    // setup object lock manager
                    ObjectLocks = new ObjectLockManager();
                    if (ObjectLocks != null)
                    {
                        // hook lock manager events
                        ObjectLocks.LogEvent += SubsystemLogEvent;

                        Log("Object lock manager initialized.");

                        // track deletes
                        UnUpdateables = new UnUpdateableChangesTracker();
                        if (UnUpdateables != null)
                        {
                            Log("UnUpdateables tracker initialized.");

                            try
                            {
                                // connect persistence layer
                                PersistenceLayer.Connect();

                                // dump persistence layer stats to log
                                PersistenceLayer.LogStats();

                                // start deletion tracker
                                UnUpdateables.Start();

                                // facilities are running
                                Running = true;

                                Log("Facilities started.");

                                return;

                            }
                            catch (Exception ex)
                            {
                                Log("Exception (" + ex.Message + ") while trying to connect to persistence data source (" + PersistenceLayer.DataSource + "). Aborting.");
                            }
                        }
                        else
                        {
                            Log("Failed to create unupdateables tracker instance. Aborting start.");
                        }
                    }
                    else
                    {
                        Log("Failed to create object lock manager instance. Aborting start.");
                    }
                }
                else
                {
                    Log("Failed to create persistence layer instance. Aborting start.");
                }
            }
            else
            {
                Log("Failed to create configuration repository instance. Aborting start.");
            }

            // facilities are not running
            Running = false;
        }

        public static void Stop()
        {
            // facilities no longer running
            Running = false;

            // stop deletion tracker
            UnUpdateables.Stop();

            // disconnect persistence layer
            PersistenceLayer.Disconnect();

            // unhook log events
            PersistenceLayer.LogEvent -= SubsystemLogEvent;
            ObjectLocks.LogEvent -= SubsystemLogEvent;
            Configuration.LogEvent -= SubsystemLogEvent;

            // drop references            
            PersistenceLayer = null;
            ObjectLocks = null;
            Configuration = null;

            Log("Facilities stopped.");
        }
        #endregion
    }
}
