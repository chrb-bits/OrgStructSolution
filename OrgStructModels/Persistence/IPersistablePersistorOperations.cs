using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgStructModels.Persistence
{
    /// <summary>
    /// Interface for internal Persistor operations on Persistables.
    /// </summary>
    internal interface IPersistablePersistorOperations
    {
        /// <summary>
        /// Internal accessor for IsDirty flag.
        /// </summary>
        /// <param name="setting">Value to set the flag to.</param>
        void SetIsDirty(bool setting);

        /// <summary>
        /// Internal accessor for IsPersistent flag.
        /// </summary>
        /// <param name="setting">Value to set the flag to.</param>
        void SetIsPersistent(bool setting);
    }
}
