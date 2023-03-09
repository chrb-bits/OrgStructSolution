using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgStructModels.Persistence
{
    internal interface IPersistorOperations
    {
        void SetIsDirty(bool setting);

        void SetIsPersistent(bool setting);
    }
}
