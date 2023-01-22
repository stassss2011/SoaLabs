using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using aggregate_service.Model;
using NuGet.Protocol;

namespace aggregate_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregateController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<string> _services = new() { "csharp", "rust" };

        public AggregateController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        // GET: api/Aggregate
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            var items = new List<LanguageModel>();
            
            foreach (var serviceName in _services)
            {
                var address = Dns.GetHostAddresses(serviceName);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://{address}/api/Language");
                var response = await httpClient.SendAsync(httpRequestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode);
                }

                items.Add(await response.Content.ReadAsAsync<LanguageModel>());
            }
            return Ok(items);
        }
    }
}
