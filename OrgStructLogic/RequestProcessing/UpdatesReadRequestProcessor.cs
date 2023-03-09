using OrgStructLogic.Service;
using OrgStructModels.Protocol;
using System;
using System.Linq;

namespace OrgStructLogic.RequestProcessing
{
    public class UpdatesReadRequestProcessor : ARequestProcessorBase<UpdatesRequest, UpdatesResult>
    {
        public override UpdatesResult Process(UpdatesRequest request)
        {
            // validate request
            if (request == null) { throw new ArgumentNullException("Null request received."); }
            if (!Facilities.Running) { throw new InvalidOperationException("Facilities not running."); }
            if (!Facilities.PersistenceLayer.IsConnected) { throw new InvalidOperationException("Persistence not connected."); }

            UpdatesResult result = new UpdatesResult();
            if (Facilities.UnUpdateables.FullRefreshRequiredAfterUTC > request.ChangesFromUTC)
            {
                // require full refresh after deletions
                result.RefreshRequired = true;
            }
            else
            {
                // find updates :)
                result.People = Facilities.PersistenceLayer.Organization.People.Where(x => x.ChangedAtUTC >= request.ChangesFromUTC).ToList();
                result.Roles = Facilities.PersistenceLayer.Organization.Roles.Where(x => x.ChangedAtUTC >= request.ChangesFromUTC).ToList();
                result.Positions = Facilities.PersistenceLayer.Organization.Positions.Where(x => x.ChangedAtUTC >= request.ChangesFromUTC).ToList();
            }
            
            result.TimestampUTC = DateTime.UtcNow;
            result.Success = true;

            return result;
        }
    }
}
