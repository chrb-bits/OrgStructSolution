using OrgStructLogic.Service;
using OrgStructModels.Protocol;
using System;

namespace OrgStructLogic.RequestProcessing
{
    public class LockReleaseRequestProcessor : ARequestProcessorBase<LockRequest, LockResult>
    {
        public override LockResult Process(LockRequest request)
        {
            // basic request validation
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (request.OperationLockID == Guid.Empty) { throw new ArgumentNullException("OperationLockID must not be empty."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not started."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            return Facilities.ObjectLocks.Release(request.OperationLockID);
        }
    }
}
