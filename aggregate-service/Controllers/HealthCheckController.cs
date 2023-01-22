using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace aggregate_service.Controllers
{
    [DisableRateLimiting]
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
