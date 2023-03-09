using OrgStructLogic.Service;
using OrgStructModels.Persistables;
using OrgStructModels.Protocol;
using System;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class RoleWriteRequestProcessor : ARequestProcessorBase<RoleRequest, RoleResult>
    {
        public override RoleResult Process(RoleRequest request)
        {
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (request.Role == null) { throw new ArgumentNullException("Person is null."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not running."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            // does specified person exist already ?
            RoleModel existingRole = Facilities.PersistenceLayer.Organization.Roles.Where(r => r.ObjectID == request.Role.ObjectID).FirstOrDefault();
            if (existingRole == null)
            {
                // does not exist, create new
                Facilities.PersistenceLayer.Organization.Roles.Add(request.Role);
                            
                // hack
                Facilities.UnUpdateables.TrackUnUpdateableChange();

                // success
                return new RoleResult(true, request.Role);
            }
            else
            {
                // exists, require valid lock
                Facilities.ObjectLocks.Validate(request.SessionID, request.Role.ObjectID, request.OperationLockID);

                // update existing person
                existingRole.CopyFrom(request.Role);

                // success                            
                return new RoleResult(true, existingRole);
            }
        }
    }
}
