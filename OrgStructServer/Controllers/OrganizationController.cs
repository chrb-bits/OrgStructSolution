using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using OrgStructModels.Persistables;
using OrgStructLogic.Service;
using OrgStructLogic.RequestProcessing;
using OrgStructModels.Protocol;

namespace OrgStructServerOWINAPI.Controllers
{
    public class OrganizationController : ApiController
    {
        // controller route
        private const string route = "organization";

        [Route(route)]
        [AcceptVerbs("Post")]
        public OrganizationResult Read(OrganizationRequest request)
        {
            OrganizationReadRequestProcessor proc = new OrganizationReadRequestProcessor();
            return proc.Process(request);
        }

        [Route(route)]
        [AcceptVerbs("Post")]
        public OrganizationResult Write(OrganizationRequest request)
        {
            throw new UnauthorizedAccessException();
        }

    }
}
