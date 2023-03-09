using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgStructClient.Session
{
    public class SessionManager
    {
        #region Data
        private Guid sessionID;
        #endregion

        #region Constructor/Destructor
        public SessionManager()
        {
            sessionID = Guid.NewGuid();
        }
        ~SessionManager()
        {
            sessionID = Guid.Empty;
        }
        #endregion

        #region Interface
        public Guid SessionID { get => sessionID; }
        #endregion
    }
}