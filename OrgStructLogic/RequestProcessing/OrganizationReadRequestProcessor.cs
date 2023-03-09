using OrgStructLogic.Service;
using OrgStructModels.Protocol;
using System;

namespace OrgStructLogic.RequestProcessing
{
    public class OrganizationReadRequestProcessor : ARequestProcessorBase<OrganizationRequest, OrganizationResult>
    {
        public override OrganizationResult Process(OrganizationRequest request)
        {
            // validate
            if (request == null) { throw new Exception("Null request."); }

            // return orgtree
            return new OrganizationResult(true, Facilities.PersistenceLayer.Organization);
        }
    }
}
