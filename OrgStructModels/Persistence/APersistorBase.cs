namespace OrgStructModels.Persistence
{
    /// <summary>
    /// Abstract base class for IPersistable persistors.
    /// </summary>
    /// <typeparam name="T">Persistable type derived from IPersistable.</typeparam>
    public abstract class APersistorBase<T> : IPersistor<T> where T:IPersistable 
    {
        #region ctor
        public APersistorBase(IPersistence persistenceLayer)
        {
            persistence = persistenceLayer;
        }
        #endregion

        #region Protected Vars and Methods
        /// <summary>
        /// Reference to persistence layer.
        /// </summary>
        protected IPersistence persistence { set; get; }

        /// <summary>
        /// Sets a Persistable's IsDirty flag.
        /// </summary>
        /// <param name="persistable">The persistable to set the flag on.</param>
        /// <param name="setting">Value to set the flag to.</param>
        protected void SetIsDirty(IPersistable persistable, bool setting)
        {
            // access internal interface IPersistorOperations
            ((IPersistablePersistorOperations)persistable).SetIsDirty(setting);
        }

        /// <summary>
        /// Sets a Persistable's IsPersitent flag.
        /// </summary>
        /// <param name="persistable">The persistable to set the flag on.</param>
        /// <param name="setting">Value to set the flag to.</param>
        protected void SetIsPersistent(IPersistable persistable, bool setting)
        {
            // access internal interface IPersistorOperations
            ((IPersistablePersistorOperations)persistable).SetIsPersistent(setting);
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Read a Persistable from persistent storage.
        /// </summary>
        /// <param name="key">Access key or object ID of the persistable to read.</param>
        /// <returns>The Persistable instance as read from persistent storage.</returns>
        public abstract T Read(string key = "");

        /// <summary>
        /// Writes a Persistable to persistent storage.
        /// </summary>
        /// <param name="persistable">The Persistable instance to write.</param>
        public abstract void Write(T persistable);

        /// <summary>
        /// Removes a Persistable from persistent storage.
        /// </summary>
        /// <param name="persistable">The Persistable instance to remove.</param>
        public abstract void Delete(T persistable);
        #endregion
    }
}
