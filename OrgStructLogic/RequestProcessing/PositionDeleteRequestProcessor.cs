using OrgStructLogic.Service;
using OrgStructModels.Persistables;
using OrgStructModels.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class PositionDeleteRequestProcessor : ARequestProcessorBase<PositionRequest, PositionResult>
    {
        public override PositionResult Process(PositionRequest request)
        {          
            // basic request validation
            if (request ==  null) { throw new ArgumentNullException("Null request received."); }
            if (request.Position == null) { throw new ArgumentNullException("Position must not be null."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not started."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            var methodTimer = System.Diagnostics.Stopwatch.StartNew();

            // find request position in local organization tree
            var sectionTimer = System.Diagnostics.Stopwatch.StartNew();
            PositionModel existingPosition = Facilities.PersistenceLayer.Organization.Positions.Where(p => p.ObjectID == request.Position.ObjectID).FirstOrDefault();
            sectionTimer.Stop();
            System.Diagnostics.Debug.WriteLine("PositionDeleteRequestProcessor.FindExistingPosition Elapsed: " + sectionTimer.ElapsedMilliseconds + "ms");
            
            if (existingPosition == null) { throw new ArgumentException("Position does not exist."); }

            // validate lock
            Facilities.ObjectLocks.Validate(request.SessionID, existingPosition.ObjectID, request.OperationLockID);

            // get other positions that contain this position in their directreports
            sectionTimer = System.Diagnostics.Stopwatch.StartNew();
            List<PositionModel> parentPositions = Facilities.PersistenceLayer.Organization.Positions.Where(p => p.DirectReports.Contains(existingPosition)).ToList();
            sectionTimer.Stop();
            System.Diagnostics.Debug.WriteLine("PositionDeleteRequestProcessor.FindParentPositions Elapsed: " + sectionTimer.ElapsedMilliseconds + "ms");

            List<Guid> parentLockIds = new List<Guid>();

            // acquire locks on parent positions (if any)
            try
            {
                foreach (PositionModel parentPosition in parentPositions)
                {
                    LockResult parentLockRes = Facilities.ObjectLocks.Acquire(request.SessionID, parentPosition.ObjectID);
                    if (parentLockRes.Success != true)
                    {
                        throw new UnauthorizedAccessException("Failed to acquire parent locks.");
                    }
                    else
                    {
                        parentLockIds.Add(parentLockRes.LockID);
                    }
                }
            }
            catch
            {
                // locking operation failed, release all parent locks
                foreach (Guid parentLockId in parentLockIds) { Facilities.ObjectLocks.Release(parentLockId); }

                // rethrow exception
                throw;
            }

            // position itself has direct reports?
            if (existingPosition.DirectReports.Count > 0)
            {

                // acquire object locks for direct reports
                List<Guid> drLocks = new List<Guid>();

                try
                {                    
                    foreach (PositionModel directReport in existingPosition.DirectReports)
                    {
                        LockResult drLockResult = Facilities.ObjectLocks.Acquire(request.SessionID, directReport.ObjectID);
                        if (drLockResult != null)
                        {
                            if (!drLockResult.Success)
                            { 
                                // locking operation failed
                                throw new Exception("Failed to acquire locks on DirectReports.");
                            }
                            else
                            {
                                drLocks.Add(drLockResult.LockID);
                            }
                        }
                        else
                        {
                            // locking operation failed
                            throw new Exception("ObjectLocks.Acquire() returned null.");

                        }
                    }
                }
                catch
                {
                    // locking operation failed, release all nested locks
                    foreach (Guid lockId in drLocks) { Facilities.ObjectLocks.Release(lockId); }

                    // rethrow exception
                    throw;
                }

                // acquired all locks, start moving directreports
                if (request.Parent != null)
                {
                    // find specified parent
                    PositionModel specifiedParent = Facilities.PersistenceLayer.Organization.Positions.Where(p => p.ObjectID == request.Parent.ObjectID).FirstOrDefault();
                    if (specifiedParent == null) { throw new ArgumentException("Parent does not exist."); }
                    // add direct reports to specified parent
                    foreach (PositionModel directReport in existingPosition.DirectReports)
                    {
                        specifiedParent.DirectReports.Add(directReport);
                    }
                }
                else
                {
                    // add direct reports to organization
                    foreach (PositionModel directReport in existingPosition.DirectReports)
                    {
                        Facilities.PersistenceLayer.Organization.Structure.Add(directReport);
                    }
                }

                // release nested locks
                foreach (Guid nestedLockId in drLocks) { Facilities.ObjectLocks.Release(nestedLockId); }
            }

            if (parentPositions.Count > 0)
            {
                foreach (PositionModel position in parentPositions)
                {
                    // remove from position
                    position.DirectReports.Remove(existingPosition);
                }
                
            }
            else
            {
                parentPositions = Facilities.PersistenceLayer.Organization.Structure.Where(p => p == existingPosition).ToList();
                foreach (PositionModel position in parentPositions)
                {
                    // remove from organization
                    Facilities.PersistenceLayer.Organization.Structure.Remove(position);
                }
            }

            // release parent locks
            foreach (Guid parentLockId in parentLockIds) { Facilities.ObjectLocks.Release(parentLockId); }

            // track unupdateable change
            Facilities.UnUpdateables.TrackUnUpdateableChange();

            methodTimer.Stop();
            System.Diagnostics.Debug.WriteLine("PositionDeleteRequestProcessor.Process() elapsed: " + methodTimer.ElapsedMilliseconds + "ms");

            // success
            return new PositionResult(true, existingPosition);
        }
    }
}
