using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using OrgStructModels.Persistables;
using OrgStructLogic.Service;
using OrgStructPersistence;
using System.Collections.Generic;
using OrgStructLogic.RequestProcessing;
using OrgStructModels.Protocol;

namespace OrgStructServerOWINAPI.Controllers
{
    public class RolesController : ApiController
    {
        // controller route
        private const string route = "roles";

        [Route(route)]
        [AcceptVerbs("Post")]
        public RoleResult Write(RoleRequest request)
        {
            RoleWriteRequestProcessor proc = new RoleWriteRequestProcessor();
            return proc.Process(request);
        }

        [Route(route)]
        [AcceptVerbs("Post")]
        public RoleResult Delete(RoleRequest request)
        {
            RoleDeleteRequestProcessor proc = new RoleDeleteRequestProcessor();
            return proc.Process(request);
        }
    }
}
