using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using OrgStructModels.Persistables;
using OrgStructLogic.Service;
using OrgStructModels.Protocol;
using OrgStructLogic.RequestProcessing;

namespace OrgStructServerOWINAPI.Controllers
{
    public class UpdatesController : ApiController
    {
        // controller route
        private const string route = "updates";

        [Route(route)]
        [AcceptVerbs("Post")]
        public UpdatesResult Read(UpdatesRequest request)
        {
            UpdatesReadRequestProcessor proc = new UpdatesReadRequestProcessor();
            return proc.Process(request);
        }
    }
}
