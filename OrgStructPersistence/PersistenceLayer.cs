using OrgStructModels.Metadata;
using OrgStructModels.Persistables;
using OrgStructModels.Persistence;
using OrgStructPersistence.Persistors;
using System;
using System.ComponentModel;
using System.IO;
using System.Timers;


namespace OrgStructPersistence
{
    /// <summary>
    /// JSON based implementation of IPersistence interface.
    /// </summary>
    public sealed class PersistenceLayer : IPersistence
    {
        #region Constructor
        public PersistenceLayer() : base()
        {
            // default organization
            organization = new OrganizationModel
            {
                Name = "Default Organization"
            };

            // default sync delay
            SyncDelayMilliseconds = 2000;
        }        
        #endregion

        #region Data
        // organization object
        private OrganizationModel organization;

        // common exceptions
        private InvalidOperationException Ex_NotConnected = new InvalidOperationException("Persistence not connected.");
        #endregion

        #region Privates
        // persistor for organization objects
        private IPersistor<OrganizationModel> orgPersistor;

        // delay flushing changes to persistence file (sync)
        private Timer syncTimer;

        // number of syncs that have been deferred since last flush
        private int deferredSyncCount = 0;
        #endregion

        #region Internals & Event Handlers
        // filestream for JSON persistence file
        internal FileStream PersistenceFileStream { private set; get; }

        // path builders for json persistence file
        internal string PersistenceFilePath { get => DataSource; }

        // log message
        internal void Log(string message)
        {
            LogEvent?.Invoke(this, new LogEventArgs(message));
        }

        private void Organization_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // some data in the organization tree changed
            if (syncTimer != null)
            {
                // max number of deferred syncs since last flush
                if (deferredSyncCount > SyncMaxDeferredChanges)
                {
                    // limit exceeded, sync now
                    System.Diagnostics.Debug.WriteLine("EXCEEDED MAX DEFERRED SYNCS");
                    Sync();
                    deferredSyncCount = 0;
                }    
                else
                {
                    // restart sync timer for deferred call
                    deferredSyncCount += 1;
                    syncTimer.Interval = SyncDelayMilliseconds;
                    syncTimer.Start();
                }
            }
        }

        private void SyncTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // sync timer elapsed, flush changes to persistence
            Sync();
        }
        #endregion

        #region Public Interface
        public event EventHandler<LogEventArgs> LogEvent;

        // DataSource is path to JSON persistence file
        public string DataSource { set; get; }

        public string DefaultOrganizationName { set; get; }

        public int SyncDelayMilliseconds { set; get; }

        public int SyncMaxDeferredChanges { set; get; }

        public bool IsConnected { private set; get; }

        public void Connect()
        {
            // drop connection if called while already connected
            if (IsConnected) { Disconnect(); }

            // obtain organization persistor
            orgPersistor = GetPersistor<OrganizationModel>();

            // sync timer
            syncTimer = new Timer()
            {
                Interval = SyncDelayMilliseconds,
                AutoReset = false
            };
            syncTimer.Elapsed += SyncTimer_Elapsed;
           
            // persistence file exists?
            if (File.Exists(PersistenceFilePath))
            {
                // yes, load it
                Log("Loading JSON persistence file (" + PersistenceFilePath + ")...");

                // open persistence filestream
                PersistenceFileStream = File.Open(PersistenceFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);                                
                
                // read organization from persistence file
                organization = orgPersistor.Read();
            }
            else
            {
                // no, create it
                Log("Creating JSON persistence file (" + PersistenceFilePath + ")...");
                
                // create persistence filestream
                PersistenceFileStream = File.Open(PersistenceFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                organization.Name = DefaultOrganizationName;

                // write organization to newly created persistence file
                orgPersistor.Write(organization);
            }

            // register for change notifications
            organization.PropertyChanged += Organization_PropertyChanged;

            // persistence is now "connected"
            IsConnected = true;
        }

        public void Disconnect()
        {
            if (!IsConnected) { return; }

            // halt synctimer, unhook elapsed event and discard
            syncTimer.Stop();
            syncTimer.Elapsed -= SyncTimer_Elapsed;
            syncTimer = null;

            // flush changes to persistent storage
            Sync();

            // discard organization persistor
            orgPersistor = null;

            // flush persistence filestream & close
            PersistenceFileStream.Flush();
            PersistenceFileStream.Close();

            // discard filestream
            PersistenceFileStream.Dispose();
            PersistenceFileStream = null;

            // discard organization reference
            organization = null;

            // persistence is no longer "connected"
            IsConnected = false;
        }

        public void Sync()
        {
            // flush if connected
            if (IsConnected)
            {
                // stop sync timer
                syncTimer?.Stop();

                // write organization to persistent storage
                orgPersistor.Write(Organization);

                // log sync
                LogEvent?.Invoke(this, new LogEventArgs("Synced."));
            }            
        }


        public OrganizationModel Organization
        {
            get
            {
                if (!IsConnected) { throw Ex_NotConnected; }
                return organization;
            }
        }

        public void LogStats()
        {
            // dump object tree stats to log
            Log("(" + Organization.People.Count + ") people, (" + Organization.Roles.Count + ") roles and " +
                "(" + Organization.Positions.Count + ") positions for organization (" + Organization.Name + ").");
        }

        public IPersistor<P> GetPersistor<P>()
            where P : IPersistable
        {
            // persistor factory method for JSON persistence layer
            if (typeof(P) == typeof(OrganizationModel))
            {
                // JSON persistence layer only supports Persistor for the OrganizationModel (which contains the entire object tree, so no other persistors are required)
                return (IPersistor<P>)new OrganizationModelPersistor(this);
            }
            else if (typeof(P) == typeof(PositionModel))
            {
                throw new NotImplementedException("GetPersistor<PositionModel>() not supported by JSON persistence layer.");
            }
            else if (typeof(P) == typeof(PersonModel))
            {
                throw new NotImplementedException("GetPersistor<PersonModel>() not supported by JSON persistence layer.");
            }
            else if (typeof(P) == typeof(RoleModel))
            {
                throw new NotImplementedException("GetPersistor<RoleModel>() not supported by JSON persistence layer.");
            }
            else
            {
                throw new ArgumentOutOfRangeException("Type not supported by persistence layer.");
            }
        }

        #endregion
    }
}
