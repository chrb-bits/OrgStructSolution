using OrgStructModels.Metadata;
using System;

namespace OrgStructModels.Protocol
{
    public class LockResult : AResultBase
    {
        public LockResult() : base()
        {
            LockID = Guid.Empty;
            LockedBySessionID = Guid.Empty;
        }

        public LockResult(bool success, Guid lockID, Guid lockedBySessionID) : base(success)
        {
            LockID = lockID;
            LockedBySessionID = lockedBySessionID;
        }
        public ObjectLock Lock { set; get; }
        public Guid LockID { set; get; }
        public Guid LockedBySessionID { set; get; }
    }
}
