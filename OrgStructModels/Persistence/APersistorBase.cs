namespace OrgStructModels.Persistence
{
    /// <summary>
    /// Abstract base class for IPersistable persistors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class APersistorBase<T> : IPersistor<T> where T:IPersistable 
    {
        protected IPersistence persistence { set; get; }

        public APersistorBase(IPersistence persistenceLayer)
        {
            persistence = persistenceLayer;
        }

        public abstract T Read(string key = "");

        public abstract void Write(T persistable);

        public abstract void Delete(T persistable);
    }
}
