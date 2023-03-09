namespace OrgStructModels.Persistence
{
    // a variant generic interface for IPersistable persistors
    public interface IPersistor<T>
        where T : IPersistable
    {   
        // read persistable from persistent storage
        T Read(string key = "");

        // write persistable to persistent storage
        void Write(T persistable);

        // delete persistable from persistent storage
        void Delete(T persistable);
    }
}
