using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using OrgStructLogic.RequestProcessing;
using OrgStructModels.Protocol;

namespace OrgStructServerOWINAPI.Controllers
{
    public class PeopleController : ApiController
    {
        // controller route
        private const string route = "people";

        [Route(route)]
        [AcceptVerbs("Post")]
        public PersonResult Write(PersonRequest request)
        {
            PersonWriteRequestProcessor proc = new PersonWriteRequestProcessor();
            return proc.Process(request);
        }

        [Route(route)]
        [AcceptVerbs("Post")]
        public PersonResult Read(PersonRequest request)
        {
            PersonReadRequestProcessor proc = new PersonReadRequestProcessor();
            return proc.Process(request);
        }

        [Route(route)]
        [AcceptVerbs("Post")]
        public PersonResult Delete(PersonRequest request)
        {
            PersonDeleteRequestProcessor proc = new PersonDeleteRequestProcessor();
            return proc.Process(request);
        }
    }
}
