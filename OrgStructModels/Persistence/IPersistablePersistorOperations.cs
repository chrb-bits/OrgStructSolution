namespace OrgStructModels.Persistence
{
    /// <summary>
    /// Internal interface for Persistor restricted operations on Persistables.
    /// </summary>
    internal interface IPersistablePersistorOperations
    {
        /// <summary>
        /// Internal accessor for IsDirty flag.
        /// </summary>
        /// <param name="setting">Value to set the flag to.</param>
        void SetIsDirty(bool setting);

        /// <summary>
        /// Internal accessor for IsPersistent flag.
        /// </summary>
        /// <param name="setting">Value to set the flag to.</param>
        void SetIsPersistent(bool setting);
    }
}
