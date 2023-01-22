using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aggregate_service.Controllers
{
    [Route("health-check")]
    [ApiController]
    public class HeathCheckController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("OK");
        }
    }
}
