using OrgStructLogic.Service;
using OrgStructModels.Persistables;
using OrgStructModels.Protocol;
using System;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class PositionWriteRequestProcessor : ARequestProcessorBase<PositionRequest, PositionResult>
    {

        public override PositionResult Process(PositionRequest request)
        {
            // validate request
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (request.Position == null) { throw new ArgumentNullException("Position is null."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not running."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            // specified position exists?
            PositionModel existingPosition = Facilities.PersistenceLayer.Organization.Positions.Where(p => p.ObjectID == request.Position.ObjectID).FirstOrDefault();
            if (existingPosition != null)
            {
                // yes, validate lock
                Facilities.ObjectLocks.Validate(request.SessionID, request.Position.ObjectID, request.OperationLockID);

                // update existing position
                existingPosition.CopyFrom(request.Position);
                //request.Position.CopyInto(existingPosition);
                
                //existingPosition.Person = request.Position.Person;
                //existingPosition.Roles = request.Position.Roles;
                //existingPosition.DirectReports = request.Position.DirectReports;

                // invalidate client side orgtrees / force full refresh
                Facilities.UnUpdateables.TrackUnUpdateableChange();

                // update successful
                return new PositionResult(true, existingPosition);
            }
            else
            {
                PositionModel parentPosition;
                if (request.Parent == null)
                {
                    // acquire organization lock
                    LockResult positionLockRes = Facilities.ObjectLocks.Acquire(request.SessionID, Facilities.PersistenceLayer.Organization.ObjectID);

                    // parent not specified (null) - add to organization
                    Facilities.PersistenceLayer.Organization.Structure.Add(request.Position);

                    // release lock
                    Facilities.ObjectLocks.Release(positionLockRes.LockID);
                }
                else
                {
                    // parent specified - find in organization
                    parentPosition = Facilities.PersistenceLayer.Organization.Positions.Where(x => x.ObjectID == request.Parent.ObjectID).FirstOrDefault();
                    if (parentPosition == null)
                    {
                        // not found
                        throw new ArgumentException("Specified parent object does not exist.");
                    }
                    else
                    {
                        // found, try to acquire lock on parent position
                        LockResult positionLockRes = Facilities.ObjectLocks.Acquire(request.SessionID, parentPosition.ObjectID);

                        // add new position
                        parentPosition.DirectReports.Add(request.Position);

                        // release lock
                        Facilities.ObjectLocks.Release(positionLockRes.LockID);
                    }
                }

                // Note: no longer required
                // invalidate client side orgtrees / force full refresh
                Facilities.UnUpdateables.TrackUnUpdateableChange();

                // add successful
                return new PositionResult(true, request.Position);
            }
        }
    }
}
