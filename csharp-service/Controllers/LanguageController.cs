using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace csharp_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        // GET: api/Language
        [HttpGet]
        public IActionResult Get()
        {
            var machineName = Environment.MachineName;
            
            return Ok(new
            {
                name = "C#",
                machineName
            });
        }
    }
}
