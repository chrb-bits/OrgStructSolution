using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace OrgStructModels.Persistence
{
    // Persistable ObjectID reference resolver for JSON.NET
    public class PersistableReferenceResolver : IReferenceResolver
    {
        private readonly IDictionary<Guid, IPersistable> _Objects = new Dictionary<Guid, IPersistable>();

        public void AddReference(object context, string reference, object value)
        {
            if (value is IPersistable)
            {
                try
                {
                    Guid id = new Guid(reference);

                    _Objects[id] = (IPersistable)value;
                }
                catch { }
            }
        }

        public string GetReference(object context, object value)
        {
            if (value is IPersistable)
            {
                IPersistable persistable = (IPersistable)value;
                _Objects[persistable.ObjectID] = persistable;

                return persistable.ObjectID.ToString();
            }
            else
            {
                return string.Empty;
            }    
        }

        public bool IsReferenced(object context, object value)
        {
            if (value is IPersistable)
            {
                IPersistable persistable = (IPersistable)value;

                return _Objects.ContainsKey(persistable.ObjectID);
            }
            else
            {
                return false;
            }
        }

        public object ResolveReference(object context, string reference)
        {
            try
            {
                Guid id = new Guid(reference);

                IPersistable persistable;
                _Objects.TryGetValue(id, out persistable);

                return persistable;
            }
            catch
            {
                return null;
            }
        }
    }
}
