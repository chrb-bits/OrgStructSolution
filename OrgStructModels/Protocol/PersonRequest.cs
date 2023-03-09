using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class PersonRequest : ARequestBase
    {
        public PersonModel Person { set; get; }
    }
}
