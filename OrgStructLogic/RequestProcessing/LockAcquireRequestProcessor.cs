using OrgStructLogic.Service;
using OrgStructModels.Protocol;
using System;

namespace OrgStructLogic.RequestProcessing
{
    public class LockAcquireRequestProcessor : ARequestProcessorBase<LockRequest, LockResult>
    {
        public override LockResult Process(LockRequest request)
        {
            // basic request validation
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not started."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            return Facilities.ObjectLocks.Acquire(request.SessionID, request.TargetObjectID);
        }
    }
}
