using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class OrganizationRequest : ARequestBase
    {
        public OrganizationRequest() : base() { }

        public OrganizationRequest(OrganizationModel organization) : base()
        {
            Organization = organization;
        }

        public OrganizationModel Organization { set; get; }
    }
}
