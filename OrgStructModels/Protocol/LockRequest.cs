using System;

namespace OrgStructModels.Protocol
{
    public class LockRequest : ARequestBase
    {
        public Guid TargetObjectID { set; get; }
    }
}
