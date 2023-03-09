using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class RoleResult : AResultBase
    {
        public RoleResult() : base() { }

        public RoleResult(bool success, RoleModel role) : base(success)
        {
            Role = role;
        }

        public RoleModel Role { set; get; }
    }
}