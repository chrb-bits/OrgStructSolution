using Newtonsoft.Json;
using System;
using System.Text;

namespace OrgStructModels.Persistables
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonModel : APersistableBase
    {
        #region Data
        [JsonProperty(PropertyName = "Name")]
        private string _Name;

        [JsonProperty(PropertyName = "FirstName")]
        private string _FirstName;
        #endregion

        #region Constructors / Destructor
        public PersonModel() : base()
        {
            Construct();
        }

        public PersonModel(Guid newObjectID) : base(newObjectID)
        {
            Construct();
        }

        private void Construct()
        {
            // initialize data fields
            _Name = string.Empty;
            _FirstName = string.Empty;
        }

        ~PersonModel()
        {
            // teardown and cleanup
            _Name = null;
            _FirstName = null;
        }
        #endregion

        #region Interface
        [JsonProperty(Order = -8)]
        public override string ObjectType => typeof(PersonModel).FullName;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Name != null)
            {
                sb.Append(Name.ToUpper());
            }
            if (FirstName != string.Empty)
            {
                if (sb.Length > 0) { sb.Append(" "); }
                sb.Append(FirstName);
            }
            if (sb.Length == 0)
            {
                sb.Append("<UNNAMED>");
            }
            return sb.ToString();
        }

        public string DisplayName { get => ToString(); }

        public string Name
        {
            set
            {
                string newValue = value.Trim();
                if (newValue != _Name)
                {
                    _Name = newValue;
                    // notify prpchg
                    OnPropertyChanged();
                    // additional prpchg notification
                    OnPropertyChangedByName("DisplayName");
                }
            }
            get
            {
                return _Name;
            }
        }
        
        public string FirstName
        {
            set
            {
                string newValue = value.Trim();
                if (newValue != _FirstName)
                {
                    _FirstName = newValue;
                    // notify prpchg
                    OnPropertyChanged();
                    // additional prpchg notification
                    OnPropertyChangedByName("DisplayName");
                }
            }
            get
            {
                return _FirstName;
            }
        }
        #endregion
    }
}
