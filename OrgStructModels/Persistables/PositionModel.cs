using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace OrgStructModels.Persistables
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class PositionModel : APersistableBase
    {
        [JsonProperty(Order = -8)]
        public override string ObjectType => (typeof(PositionModel).FullName);

        #region Data
        [JsonProperty(PropertyName = "Person")]
        PersonModel _Person;

        [JsonProperty(PropertyName = "Roles")]
        BindingList<RoleModel> _Roles;

        [JsonProperty(PropertyName = "DirectReports")]
        BindingList<PositionModel> _DirectReports;
        #endregion

        #region Constructors / Destructor
        public PositionModel() : base()
        {
            Construct();
        }

        public PositionModel(Guid newObjectID) : base(newObjectID)
        {
            Construct();
        }

        private void Construct()
        {
            _Person = new PersonModel();
            _Person.PropertyChanged += Person_PropertyChanged;
            
            _Roles = new BindingList<RoleModel>();
            _Roles.ListChanged += Roles_ListChanged;
            
            _DirectReports = new BindingList<PositionModel>();
            _DirectReports.ListChanged += DirectReports_ListChanged;
        }

        ~PositionModel()
        {
            if (_Person != null)
            {
                _Person.PropertyChanged -= Person_PropertyChanged;
                _Person = null;
            }

            if (_Roles != null)
            {
                _Roles.ListChanged -= Roles_ListChanged;
                _Roles.Clear();
                _Roles = null;
            }

            if (_DirectReports != null)
            {
                _DirectReports.ListChanged -= DirectReports_ListChanged;
                _DirectReports = null;
            }
        }
        #endregion

        #region Internals and Privates
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            // unhook events before deserialization
            if (_Person != null) { _Person.PropertyChanged -= Person_PropertyChanged; }
            if (_Roles != null) { _Roles.ListChanged -= Roles_ListChanged; }
            if (_DirectReports != null) { _DirectReports.ListChanged -= DirectReports_ListChanged; }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // rehook events after deserialization
            if (_Person != null) { _Person.PropertyChanged += Person_PropertyChanged; }
            if (_Roles != null) { _Roles.ListChanged += Roles_ListChanged; }
            if (_DirectReports != null) { _DirectReports.ListChanged += DirectReports_ListChanged; }
        }

        private void DirectReports_ListChanged(object sender, ListChangedEventArgs e)
        {
            OnPropertyChangedByName("DirectReports");
        }

        private void Roles_ListChanged(object sender, ListChangedEventArgs e)
        {
            // update position name as well
            OnPropertyChangedByName("Roles");
            OnPropertyChangedByName("Name");
        }

        private void Person_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // update position name as well
            OnPropertyChangedByName("Person");
            OnPropertyChangedByName("Name");
        }
        #endregion

        #region Public Interface
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Person != null)
            {
                sb.Append(Person.ToString());
            }
            else
            {
                sb.Append("<UNASSIGNED>");
            }
            if (Roles.Count > 0)
            {                
                sb.Append(" (");
                int roleCount = 1;
                foreach (RoleModel role in Roles)
                {
                    sb.Append(role.ToString());
                    if (roleCount < Roles.Count) { sb.Append(", "); }
                    roleCount++;
                }
                sb.Append(")");
            }
            return sb.ToString();
        }

        public string Name
        {
            set
            {
                // ignore
            }
            get
            {
                return ToString();
            }
        }

        public PersonModel Person
        {
            set
            {
                if (_Person != null)
                {
                    _Person.PropertyChanged -= Person_PropertyChanged;
                }                
                _Person = value;
                if (_Person != null)
                {
                    _Person.PropertyChanged += Person_PropertyChanged;
                }                
                OnPropertyChanged();
            }
            get
            {
                return _Person;
            }
        }

        public BindingList<RoleModel> Roles
        {
            set
            {
                if (_Roles != null)
                {
                    _Roles.ListChanged -= Roles_ListChanged;
                }
                _Roles = value;
                if (_Roles != null)
                {
                    _Roles.ListChanged += Roles_ListChanged;
                }
                OnPropertyChanged();

            }
            get
            {
                return _Roles;
            }            
        }

        public BindingList<PositionModel> DirectReports
        {
            set
            {
                if (_DirectReports != null)
                {
                    _DirectReports.ListChanged -= DirectReports_ListChanged;
                }
                _DirectReports = value;
                if (_DirectReports != null)
                {
                    _DirectReports.ListChanged += DirectReports_ListChanged;
                }
                OnPropertyChanged();
            }
            get
            {
                return _DirectReports;
            }
        }
        #endregion
    }
}
