using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using OrgStructModels.Protocol;
using OrgStructLogic.Service;
using OrgStructLogic.RequestProcessing;

namespace OrgStructServerOWINAPI.Controllers
{
    public class PositionsController : ApiController
    {
        // controller route
        private const string route = "positions";

        [Route(route)]
        [AcceptVerbs("Post")]
        public PositionResult Write(PositionRequest request)
        {
            PositionWriteRequestProcessor proc = new PositionWriteRequestProcessor();
            return proc.Process(request);
        }

        [Route(route)]
        [AcceptVerbs("Post")]
        public PositionResult Delete(PositionRequest request)
        {
            PositionDeleteRequestProcessor proc = new PositionDeleteRequestProcessor();
            return proc.Process(request);
        }

        //[Route(route)]
        //[AcceptVerbs("Post")]
        //public PositionResult Read(PositionRequest request)
        //{
        //    PositionReadRequestProcessor proc = new PositionReadRequestProcessor();
        //    return proc.Process(request);
        //}
    }
}
