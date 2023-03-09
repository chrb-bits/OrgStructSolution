using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class RoleRequest : ARequestBase
    {
        public RoleModel Role { set; get; }
    }
}
