using OrgStructLogic.Service;
using OrgStructModels.Persistables;
using OrgStructModels.Protocol;
using System;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class PersonWriteRequestProcessor : ARequestProcessorBase<PersonRequest, PersonResult>
    {
        public override PersonResult Process(PersonRequest request)
        {
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (request.Person == null) { throw new ArgumentNullException("Person is null."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not running."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            // does specified person exist already ?
            PersonModel existingPerson = Facilities.PersistenceLayer.Organization.People.Where(p => p.ObjectID == request.Person.ObjectID).FirstOrDefault();
            if (existingPerson == null)
            {
                // does not exist, create new
                Facilities.PersistenceLayer.Organization.People.Add(request.Person);

                // success
                return new PersonResult(true, request.Person);
            }
            else
            {
                // exists, require valid lock
                Facilities.ObjectLocks.Validate(request.SessionID, request.Person.ObjectID, request.OperationLockID);

                // update existing person
                existingPerson.CopyFrom(request.Person);

                // success                            
                return new PersonResult(true, existingPerson);
            }
        }
    }
}
