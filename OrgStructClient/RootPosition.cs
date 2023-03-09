using OrgStructModels.Persistables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgStructClient
{
    /// <summary>
    ///  A PositionModel wrapper for use as TreeView root item
    /// </summary>
    public class RootPosition : PositionModel, IEnumerable<PositionModel>
    {
        public string OrgName { set; get; }

        public IEnumerator<PositionModel> GetEnumerator()
        {
            return DirectReports.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            return OrgName;
        }
    }
}
