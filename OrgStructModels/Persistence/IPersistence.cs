using OrgStructModels.Metadata;
using OrgStructModels.Persistables;
using System;
using System.ComponentModel;

namespace OrgStructModels.Persistence
{   
    /// <summary>
    /// Interface for persistence layer implementations.
    /// </summary>
    public interface IPersistence
    {
        /// <summary>
        /// Persistence log message event.
        /// </summary>
        event EventHandler<LogEventArgs> LogEvent;

        /// <summary>
        /// Path to the persistent storage. Could be a file path, a network path, a connection string - depending on the actual Persistence Layer implementation.
        /// </summary>
        string DataSource { set; get; }

        /// <summary>
        /// True indicates that the Persistence Layer is currently connected to persistent storage.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Calls to Sync() on change to the object tree will be delayed for SyncDelayMilliseconds after the last call.
        /// The delay timer gets reset each time a new change is registered. This prevents excessive writes when multiple changes are applied in rapid succession.
        /// If the number of changes since last synchronization exceeds SyncMaxDeferredChanges, Sync() will be invoked immediately, regardless of the delay.
        /// </summary>
        int SyncDelayMilliseconds { set; get; }

        /// <summary>
        /// Maxmimum number of unsynchronized changes to the in-memory object tree, before immediate sync is performed on any change.
        /// </summary>
        int SyncMaxDeferredChanges { set; get; }

        /// <summary>
        /// Default name to use for new organization.
        /// </summary>
        string DefaultOrganizationName { set; get; }

        /// <summary>
        /// In-Memory object tree
        /// </summary>
        OrganizationModel Organization { get; }

        /// <summary>
        /// Connect to persistent storage.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from persistent storage.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Flush all in-memory object tree changes to persistent storage.
        /// </summary>
        void Sync();

        /// <summary>
        /// Dump persistence stats to log.
        /// </summary>
        void LogStats();

        /// <summary>
        /// Factory method for Persistors. Manufactures an appropriate Persistor for the Persistable type, or throws an exception if the Persistable type is not supported.
        /// </summary>
        /// <typeparam name="P">Persistable type derived from IPersistable.</typeparam>
        /// <returns>A Persistor instance for the Persistable type.</returns>
        IPersistor<P> GetPersistor<P>() where P : IPersistable;

    }
}
