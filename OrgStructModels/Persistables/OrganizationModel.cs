using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OrgStructModels.Persistables
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganizationModel : APersistableBase
    {
        #region Data
        [JsonProperty(PropertyName = "Name")]
        private string _Name;

        [JsonProperty(PropertyName = "People")]
        private BindingList<PersonModel> _People;

        [JsonProperty(PropertyName = "Roles")]
        private BindingList<RoleModel> _Roles;

        [JsonProperty(PropertyName = "Structure")]
        private BindingList<PositionModel> _Structure;

        private BindingList<PositionModel> positionsCache;
        #endregion

        #region Constructors/Destructor
        public OrganizationModel() : base()
        {
            Construct();
        }

        public OrganizationModel(Guid newObjectID) : base(newObjectID)
        {
            Construct();
        }

        private void Construct()
        {
            // initialize data fields
            _Name = string.Empty;
            _People = new BindingList<PersonModel>();
            _Roles = new BindingList<RoleModel>();
            _Structure = new BindingList<PositionModel>();
            positionsCache = null;

            // register for changes in lists
            _People.ListChanged += People_ListChanged;
            _Roles.ListChanged += Roles_ListChanged;
            _Structure.ListChanged += Structure_ListChanged;
        }

        ~OrganizationModel()
        {
            // unregister list changed events and clear/destroy lists
            if (_People != null)
            {
                _People.ListChanged -= People_ListChanged;
                _People.Clear();
                _People = null;
            }

            if (_Roles != null)
            {
                _Roles.ListChanged -= Roles_ListChanged;
                _Roles.Clear();
                _Roles = null;
            }

            if (_Structure != null)
            {
                _Structure.ListChanged -= Structure_ListChanged;
                _Structure.Clear();
                _Structure = null;
            }
            
            if (positionsCache != null)
            {
                positionsCache.Clear();
                positionsCache = null;
            }
        }
        #endregion

        #region Internals and Privates
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            // unhook events before deserialization
            if (_People != null) { _People.ListChanged -= People_ListChanged; }
            if (_Roles != null) { _Roles.ListChanged -= Roles_ListChanged; }
            if (_Structure != null) { _Structure.ListChanged -= Structure_ListChanged; }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // rehook events after deserialization
            if (_People != null) { _People.ListChanged += People_ListChanged; }
            if (_Roles != null) { _Roles.ListChanged += Roles_ListChanged; }
            if (_Structure != null) { _Structure.ListChanged += Structure_ListChanged; }
        }

        private void GetPositionsFlat(PositionModel startingPosition, BindingList<PositionModel> positionsList)
        {
            // parallelized (experimental)
            Parallel.ForEach(startingPosition.DirectReports, directReport =>
            {
                // bindinglist not thread-safe
                // todo: evaluate if this lock doesnt synchronize and defeat parallelism
                lock(positionsList)
                {
                    positionsList.Add(directReport);
                }

                // has descendant positions?
                if (directReport.DirectReports.Count > 0)
                {
                    // recursively find them!
                    GetPositionsFlat(directReport, positionsList);
                }
            });
        }

        private void Roles_ListChanged(object sender, ListChangedEventArgs e)
        {
            // notify prpchg
            OnPropertyChangedByName("Roles");
        }

        private void People_ListChanged(object sender, ListChangedEventArgs e)
        {
            // notify prpchg
            OnPropertyChangedByName("People");
        }
        private void Structure_ListChanged(object sender, ListChangedEventArgs e)
        {
            // notify prpchg
            OnPropertyChangedByName("Structure");
            OnPropertyChangedByName("Positions");
            
            // invalidate positions cache
            if (positionsCache != null)
            {
                positionsCache.Clear();
                positionsCache = null;
                //System.Diagnostics.Debug.WriteLine("POSITIONS CACHE INVALIDATED.");
            }
        }
        #endregion

        #region Interface        
        [JsonProperty(Order = -8)]
        public override string ObjectType => (typeof(OrganizationModel).FullName);

        public string Name
        {
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    OnPropertyChanged();
                }
            }
            get
            {
                return _Name;
            }
        }

        public BindingList<RoleModel> Roles { get => _Roles; }

        public BindingList<PersonModel> People { get => _People; }

        public BindingList<PositionModel> Structure { get => _Structure; }

        public BindingList<PositionModel> Positions
        {
            get
            {   
                // list cached? return from cache, if so.
                if (positionsCache != null) { return new BindingList<PositionModel>(positionsCache); }

                var timer = System.Diagnostics.Stopwatch.StartNew();

                // rebuild positions list
                BindingList<PositionModel> positionsCacheNew = new BindingList<PositionModel>();                
                foreach (PositionModel position in Structure)
                {
                    // add
                    positionsCacheNew.Add(position);

                    // has descendants?
                    if (position.DirectReports.Count > 0)
                    {
                        // recursively find them
                        GetPositionsFlat(position, positionsCacheNew);
                    }                    
                }

                // update cache
                positionsCache = positionsCacheNew;

                timer.Stop();
                System.Diagnostics.Debug.WriteLine("POSITIONS CACHE REBUILT IN " + timer.ElapsedMilliseconds + "ms");

                // return list
                return new BindingList<PositionModel>(positionsCache);
            }
        }
        #endregion
    }
}
