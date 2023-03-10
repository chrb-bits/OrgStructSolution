using OrgStructLogic.Service;
using OrgStructModels.Metadata;
using OrgStructModels.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OrgStructLogic.ObjectManagement
{
    public class ObjectLockManager
    {
        #region Data
        // our list of object locks
        private List<ObjectLock> objectLocks;

        private UnauthorizedAccessException Ex_Unauthorized = new UnauthorizedAccessException("Lock is invalid.");
        #endregion

        #region Constructor/Destructor
        public ObjectLockManager()
        {
            objectLocks = new List<ObjectLock>();
        }        

        ~ObjectLockManager()
        {
            objectLocks.Clear();
            objectLocks = null;
        }
        #endregion

        #region Privates
        private void Log(string message)
        {
            LogEvent?.Invoke(this, new LogEventArgs(message));
        }
        #endregion

        #region Interface
        public event EventHandler<LogEventArgs> LogEvent;

        public void Validate(Guid sessionID, Guid targetID, Guid lockID)
        {          
            if (lockID != Guid.Empty)
            {
                // find lock by ID and validate
                //ObjectLock existingLock = objectLocks.Where(x => x.LockID == lockID).FirstOrDefault();
                ObjectLock existingLock = objectLocks.Where(l => l.LockID == lockID).FirstOrDefault();
                if (existingLock != null)
                {
                    if (existingLock.SessionID == sessionID && existingLock.TargetID == targetID)
                    {
                        // seems kosher, exit without exception
                        return;
                    }
                }
            }

            // lock not validated, throw exception
            throw Ex_Unauthorized;            
        }
        
        // acquire object lock
        public LockResult Acquire(Guid sessionID, Guid objectID, [CallerMemberName] string callerName = "")
        {
            // pre-existing lock for object?
            ObjectLock existingLock = objectLocks.Where(x => x.TargetID == objectID).FirstOrDefault();
            if (existingLock != null)
            {
                // lock exists
                if (existingLock.SessionID == sessionID)
                {
                    // locked by requesting session, refresh lock
                    existingLock.Refresh(sessionID);

                    Log("Lock (" + existingLock.LockID + ") reacquired by (" + sessionID + ").");

                    // success
                    return new LockResult(true, existingLock.LockID, existingLock.SessionID);
                }
                else
                {
                    // locked by another session, determine lock age
                    TimeSpan lockDuration = DateTime.UtcNow.Subtract(existingLock.LockAcquiredAtUTC);
                    
                    // lock expired?
                    if (lockDuration.TotalSeconds > Facilities.Configuration.Service.ObjectLockTimeout.TotalSeconds)
                    {
                        // existing lock timed out - acquire for current session
                        existingLock.Acquire(sessionID, objectID);

                        Log("Lock (" + existingLock.LockID + ") acquired by (" + sessionID + ").");

                        // success
                        return new LockResult(true, existingLock.LockID, existingLock.SessionID);
                    }
                    else
                    {
                        Log("Lock acquistion by (" + sessionID + ") failed (locked by foreign session).");

                        // and lock is still valid - reject acquisition
                        return new LockResult(false, existingLock.LockID, existingLock.SessionID);
                    }
                }
            }
            else
            {
                // lock does not exist yet, acquire for session
                ObjectLock newLock = new ObjectLock();
                newLock.Acquire(sessionID, objectID);
                objectLocks.Add(newLock);

                Log("Lock (" + newLock.LockID + ") acquired by session (" + sessionID + ").");

                // success
                return new LockResult(true, newLock.LockID, newLock.SessionID);
            }
        }

        // release object lock by lock ID
        public LockResult Release(Guid lockID)
        {
            ObjectLock existingLock = objectLocks.Where(x => x.LockID == lockID).FirstOrDefault();
            if (existingLock != null)
            {
                // lock ID exists, release it
                return Release(existingLock);
            }
            else
            {
                // lock ID does not exist, fail
                return new LockResult(false, Guid.Empty, Guid.Empty);
            }
        }

        public LockResult Release(ObjectLock lockToRelease)
        {
            if (objectLocks.Contains(lockToRelease))
            {
                // drop lock
                objectLocks.Remove(lockToRelease);

                Log("Lock (" + lockToRelease.LockID + ") released.");

                // success
                return new LockResult(true, Guid.Empty, Guid.Empty);
            }
            else
            {
                return new LockResult(false, Guid.Empty, Guid.Empty);
            }
        }

        // release all locks for specified session
        public LockResult ReleaseAll(Guid sessionID)
        {
            // get object locks for requesting session
            List<ObjectLock> sessionLocks = objectLocks.Where(x => x.SessionID == sessionID).ToList();

            if (sessionLocks.Count > 0)
            {
                // clear session locks
                while (sessionLocks.Count > 0)
                {
                    // remove from object locks list
                    objectLocks.Remove(sessionLocks[0]);

                    // remove from session locks list
                    sessionLocks.RemoveAt(0);
                }

                Log("All locks for session (" + sessionID + ") released.");

                // successfully released
                return new LockResult(true, Guid.Empty, Guid.Empty);
            }
            else
            {
                // there were no locks to release
                return new LockResult(false, Guid.Empty, Guid.Empty);
            }
        }

        // release expired locks
        public int Scavenge()
        {
            // purge list
            List<ObjectLock> expiredLocks = new List<ObjectLock>();
                        
            // scan for expired locks
            foreach (ObjectLock existingLock in objectLocks)
            {
                // determine lock age
                TimeSpan lockDuration = DateTime.UtcNow.Subtract(existingLock.LockAcquiredAtUTC);

                // expired?
                if (lockDuration.TotalSeconds > Facilities.Configuration.Service.ObjectLockTimeout.TotalSeconds)
                {
                    // yup, add to purge list
                    expiredLocks.Add(existingLock);
                }
            }

            // return value
            int scavengedCount = expiredLocks.Count;

            // process purge list
            while (expiredLocks.Count > 0)
            {
                // discard object lock
                objectLocks.Remove(expiredLocks[0]);
                
                // remove from purge list
                expiredLocks.RemoveAt(0);
            }

            return scavengedCount;
        }

        // release all locks
        public void Purge()
        {
            objectLocks.Clear();
        }
        #endregion
    }

}
