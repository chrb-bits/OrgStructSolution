using Newtonsoft.Json;
using OrgStructModels.Persistence;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OrgStructModels.Persistables
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class APersistableBase : IPersistable, INotifyPropertyChanged, IPersistorOperations
    {
        #region Constructors
        public APersistableBase()
        {
            // create new object ID
            ObjectID = Guid.NewGuid();
            Construct();
        }

        public APersistableBase(Guid newObjectID)
        {
            // use supplied object ID
            ObjectID = newObjectID;
            Construct();
        }

        private void Construct()
        {
            // initialize data properties
            ChangedAtUTC = DateTime.MinValue;
            IsDirty = false;
            IsPersistent = false;
        }
        #endregion

        #region Protected
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            ChangedAtUTC = DateTime.UtcNow;
            IsDirty = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnPropertyChangedByName(string propertyName)
        {
            ChangedAtUTC = DateTime.UtcNow;
            IsDirty = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Data / Public Interface
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The globally unique object identifier of this Persistable (Guid V4).
        /// </summary>
        [JsonProperty(Order =-9)]
        public Guid ObjectID { internal set; get; }

        /// <summary>
        /// Type name of this Persistable, as string. For developer's/admin's convenience only.
        /// </summary>
        [JsonProperty(Order = -8)]
        public abstract string ObjectType { get; }

        /// <summary>
        /// Timestamp (UTC) of last change to this Persistable.
        /// </summary>
        [JsonProperty]
        public DateTime ChangedAtUTC { set; get; }

        /// <summary>
        /// Set to true by the Persistable when a property of this Persistable has changed.
        /// Set to false by Persistor when the Persistable is written to persistent storage.
        /// </summary>
        public bool IsDirty { private set; get; }
        
        /// <summary>
        /// True indicates that this Persistable exists in persistent storage.
        /// </summary>
        public bool IsPersistent { private set; get; }

        /// <summary>
        /// Copies data from this Persistable into a target Persistable, preserving the target Persistable's objectID.
        /// </summary>
        /// <typeparam name="P">Persistable type derived from IPersistable.</typeparam>
        /// <param name="target">The target Persistable to copy data into.</param>
        public void CopyInto<P>(P target) where P : IPersistable
        {            
            Guid localID = ObjectID;
            Guid targetID = target.ObjectID;

            // serialize local
            string json = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { ReferenceResolverProvider = () => new PersistableReferenceResolver() });
            
            // magic
            json = json.Replace(localID.ToString(), targetID.ToString());
            
            // deserialize into target
            JsonConvert.PopulateObject(json, target, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, ReferenceResolverProvider = () => new PersistableReferenceResolver() });

            // verify magic
            System.Diagnostics.Debug.Assert(target.ObjectID == targetID);

            // mark target as dirty, notify changes
            target.Spoil();
        }

        /// <summary>
        /// Copies data from a source Persistable into this Persistable, preserving this Persistable's object ID.
        /// </summary>
        /// <typeparam name="P">Persistable type derived from IPersistable.</typeparam>
        /// <param name="source">The source Persitable to copy data from.</param>
        public void CopyFrom<P>(P source) where P : IPersistable
        {
            Guid localID = ObjectID;
            Guid sourceID = source.ObjectID;

            // serialize source
            string json = JsonConvert.SerializeObject(source, Formatting.None, new JsonSerializerSettings { ReferenceResolverProvider = () => new PersistableReferenceResolver() });

            // magic
            json = json.Replace(sourceID.ToString(), localID.ToString());

            // deserialize into local
            JsonConvert.PopulateObject(json, this, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, ReferenceResolverProvider = () => new PersistableReferenceResolver() });

            // verify magic
            System.Diagnostics.Debug.Assert(ObjectID == localID);

            // mark local as dirty, notify changes
            this.Spoil(); 
        }

        /// <summary>
        /// Creates a clone (exact copy, with the same objectID as the source) of this Persistable.
        /// </summary>
        /// <typeparam name="P">Persistable type derived from IPersitable.</typeparam>
        /// <returns>Clone of the Persistable.</returns>
        public P Clone<P>() where P : IPersistable
        {
            string json = JsonConvert.SerializeObject(this);
            P clone = JsonConvert.DeserializeObject<P>(json);
            return clone;
        }

        /// <summary>
        /// Mark this persistable as dirty, update changed timestamp and notify all properties changed.
        /// </summary>
        public void Spoil()
        {
            IsDirty = true;
            ChangedAtUTC = DateTime.UtcNow;
            OnPropertyChangedByName(string.Empty);
        }

        void IPersistorOperations.SetIsDirty(bool setting)
        {
            IsDirty = setting;
        }

        void IPersistorOperations.SetIsPersistent(bool setting)
        {
            IsPersistent = setting;
        }
        #endregion
    }
}
