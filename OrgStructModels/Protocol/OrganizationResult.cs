using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class OrganizationResult : AResultBase
    {
        public OrganizationResult() : base () { }

        public OrganizationResult(bool success, OrganizationModel organization) : base ()
        {
            Success = success;
            Organization = organization;
        }

        public OrganizationModel Organization { set; get; }
    }
}
