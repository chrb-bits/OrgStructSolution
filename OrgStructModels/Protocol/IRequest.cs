using System;

namespace OrgStructModels.Protocol
{
    public interface IRequest
    {
        Guid SessionID { set; get; }
    }
}
