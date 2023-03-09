using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OrgStructModels.Persistables
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class RoleModel : APersistableBase
    {
        #region Data
        [JsonProperty(PropertyName = "Name")]
        private string _Name;
        #endregion

        #region Constructors
        public RoleModel() : base()
        {
            Construct();
        }

        public RoleModel(Guid newObjectID) : base(newObjectID)
        {
            Construct();
        }

        private void Construct()
        {
            // initialize data fields
            _Name = string.Empty;
        }      
        #endregion

        #region Interface
        [JsonProperty(Order = -8)]
        public override string ObjectType => (typeof(RoleModel).FullName);

        public override string ToString()
        {
            return Name;
        }

        public string Name
        {
            set
            {
                string newValue = value.Trim();
                if (newValue != _Name)
                {
                    _Name = newValue;
                    OnPropertyChanged();
                }
            }
            get
            {
                return _Name;
            }
        }
        #endregion
    }
}
