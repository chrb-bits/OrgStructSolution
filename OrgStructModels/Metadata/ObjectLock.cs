using System;
using System.Security.Cryptography;

namespace OrgStructModels.Metadata
{
    public class ObjectLock
    {
        #region Constructors
        public ObjectLock()
        {
            LockID = new Guid();
        }

        public ObjectLock(Guid lockID)
        {
            LockID = lockID;
        }
        #endregion

        #region Privates
        // create a cryptographically secure GUID
        private Guid CreateSecureGUID()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                // get random bytes from crypto RNG provider
                var bytes = new byte[16];
                provider.GetBytes(bytes);

                // set GUID version 4
                bytes[8] = (byte)(bytes[8] & 0xbf | 0x80);
                bytes[7] = (byte)(bytes[7] & 0x4f | 0x40);
                
                // done
                return new Guid(bytes);
            }
        }
        #endregion

        #region Interface
        // acquire new lock for session
        public void Acquire(Guid sessionID, Guid targetObjectID)
        {
            LockID = CreateSecureGUID();
            SessionID = sessionID;
            TargetID = targetObjectID;
            LockAcquiredAtUTC = DateTime.UtcNow;
        }

        // refresh lock for session
        public void Refresh(Guid sessionID)
        {
            if (sessionID == SessionID)
            {
                // requesting session is owner, refresh lock
                LockAcquiredAtUTC = DateTime.UtcNow;
            }
            else
            {
                // lock does not belong to requesting session, reject refresh
                throw new UnauthorizedAccessException("Lock not owned by requesting sessionID.");
            }
        }

        // the id of this lock
        public Guid LockID { private set; get; }
       
        // the id of the object this lock applies to        
        public Guid TargetID { set; get; }
        
        // the owner of this lock
        public Guid SessionID { set; get; }

        // when the lock was last (re-)acquired
        public DateTime LockAcquiredAtUTC { set; get; }
        #endregion
    }
}
