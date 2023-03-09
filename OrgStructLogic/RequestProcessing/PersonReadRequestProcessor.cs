using OrgStructLogic.Service;
using OrgStructModels.Protocol;
using System;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class PersonReadRequestProcessor : ARequestProcessorBase<PersonRequest, PersonResult>
    {
        public override PersonResult Process(PersonRequest request)
        {
            if (request == null) { throw new ArgumentException("Null request."); }

            return new PersonResult(Facilities.PersistenceLayer.Organization.People.Where(p => p.ObjectID == request.Person.ObjectID).FirstOrDefault());
        }
    }
}
