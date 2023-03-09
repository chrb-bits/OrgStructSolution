using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using OrgStructModels.Protocol;
using OrgStructLogic.RequestProcessing;
using OrgStructLogic.Service;

namespace OrgStructServerOWINAPI.Controllers
{
    public class LocksController : ApiController
    {
        // controller route
        private const string route = "locks";

        [Route(route)]
        [AcceptVerbs("Post")]
        public LockResult Acquire(LockRequest request)
        {
            LockAcquireRequestProcessor proc = new LockAcquireRequestProcessor();
            return proc.Process(request);
        }

        [Route(route)]
        [AcceptVerbs("Post")]
        public LockResult Release(LockRequest request)
        {
            LockReleaseRequestProcessor proc = new LockReleaseRequestProcessor();
            return proc.Process(request);
        }

        [Route(route)]
        [AcceptVerbs("Post")]
        public LockResult ReleaseAll(LockRequest request)
        {
            LockReleaseAllRequestProcessor proc = new LockReleaseAllRequestProcessor();
            return proc.Process(request);
        }
    }
}
