using System;

namespace OrgStructLogic.ObjectManagement
{
    public class UnUpdateableChangesTracker
    {
        public UnUpdateableChangesTracker()
        {
            FullRefreshRequiredAfterUTC = DateTime.MinValue;
        }

        public DateTime FullRefreshRequiredAfterUTC { private set; get; }

        public void Start()
        {
            FullRefreshRequiredAfterUTC = DateTime.UtcNow;
        }

        public void Stop()
        {
            FullRefreshRequiredAfterUTC = DateTime.MinValue;
        }

        public void TrackUnUpdateableChange()
        {
            FullRefreshRequiredAfterUTC = DateTime.UtcNow;
        }
    }
}
