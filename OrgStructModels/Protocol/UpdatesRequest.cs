using System;

namespace OrgStructModels.Protocol
{
    public class UpdatesRequest : ARequestBase
    {
        public DateTime ChangesFromUTC { set; get; }
    }
}
