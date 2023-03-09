using OrgStructModels.Persistables;
using System;
using System.Collections.Generic;

namespace OrgStructModels.Protocol
{
    public class UpdatesResult : AResultBase
    {
        public UpdatesResult() : base()
        {
            People = new List<PersonModel>();
            Roles = new List<RoleModel>();
            Positions = new List<PositionModel>();
        }

        ~UpdatesResult()
        {
            People = null;
            Roles = null;
            Positions = null;
        }

        public DateTime TimestampUTC { set; get; }
        public bool RefreshRequired { set; get; }
        public string OrganizationName { set; get; }
        public List<PersonModel> People { set; get; }
        public List<RoleModel> Roles { set; get; }
        public List<PositionModel> Positions { set; get; }
    }
}
