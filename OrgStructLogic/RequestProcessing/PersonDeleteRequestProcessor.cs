using OrgStructLogic.Service;
using OrgStructModels.Persistables;
using OrgStructModels.Protocol;
using System;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class PersonDeleteRequestProcessor : ARequestProcessorBase<PersonRequest, PersonResult>
    {
        public override PersonResult Process(PersonRequest request)
        {
            // basic request validation
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (request.Person == null) { throw new ArgumentNullException("Person must not be null."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not started."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            // find request position in organization tree
            PersonModel existingPerson = Facilities.PersistenceLayer.Organization.People.Where(p => p.ObjectID == request.Person.ObjectID).FirstOrDefault();
            if (existingPerson == null) { throw new ArgumentException("Person does not exist."); }

            // validate lock on person
            Facilities.ObjectLocks.Validate(request.SessionID, existingPerson.ObjectID, request.OperationLockID);

            // unassign person from any positions that have it assigned - todo: lock check on positions
            foreach (PositionModel position in Facilities.PersistenceLayer.Organization.Positions)
            {
                if (position.Person == existingPerson)
                {
                    position.Person = null;
                }
            }

            // remove role from organization
            Facilities.PersistenceLayer.Organization.People.Remove(existingPerson);

            // track unupdateable
            Facilities.UnUpdateables.TrackUnUpdateableChange();

            // success
            return new PersonResult(true, existingPerson);
        }
    }
}
