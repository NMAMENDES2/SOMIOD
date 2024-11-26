using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SOMIOD.Controllers
{

    public class ContainerController : ApiController
    {

        string connStr = Properties.Settings.Default.ConString;


        

        [Route("api/somiod/containers")]
        [HttpGet]
        public List<Container> GetAllContainers()
        {
            List <Container> = new List<Container>;
        }
    }
}
