using System;

namespace OrgStructModels.Protocol
{
    public abstract class ARequestBase : IRequest
    {
        public Guid SessionID { set; get; }

        public Guid OperationLockID { set; get; }
    }
}
