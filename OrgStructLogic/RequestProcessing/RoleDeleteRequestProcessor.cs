using OrgStructLogic.Service;
using OrgStructModels.Persistables;
using OrgStructModels.Protocol;
using System;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class RoleDeleteRequestProcessor : ARequestProcessorBase<RoleRequest, RoleResult>
    {
        public override RoleResult Process(RoleRequest request)
        {
            // basic request validation
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (request.Role == null) { throw new ArgumentNullException("Role must not be null."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not started."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            // find request position in organization tree
            RoleModel existingRole = Facilities.PersistenceLayer.Organization.Roles.Where(p => p.ObjectID == request.Role.ObjectID).FirstOrDefault();
            if (existingRole == null) { throw new ArgumentException("Role does not exist."); }

            // validate lock
            Facilities.ObjectLocks.Validate(request.SessionID, existingRole.ObjectID, request.OperationLockID);

            // unassign role from any positions that have it - todo: lock check on positions
            foreach (PositionModel position in Facilities.PersistenceLayer.Organization.Positions)
            {
                if (position.Roles.Contains(existingRole))
                {
                    position.Roles.Remove(existingRole);
                }
            }

            // remove role from organization
            Facilities.PersistenceLayer.Organization.Roles.Remove(existingRole);

            // track unupdateable
            Facilities.UnUpdateables.TrackUnUpdateableChange();

            // success
            return new RoleResult(true, existingRole);
        }
    }
}
