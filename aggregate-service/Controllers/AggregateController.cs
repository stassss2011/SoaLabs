using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using aggregate_service.Model;
using Consul;
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

            using var client = new ConsulClient(new ConsulClientConfiguration
            {
                Address = new Uri("http://consul:8500")
            });

            var services = await client.Agent.Services();
            var rand = new Random();
            var lngServices = services.Response.Values
                .Where(srv => _services
                    .Any(sn => srv.Service.Contains(sn)))
                .GroupBy(srv => srv.Service)
                .Select(srv => srv
                    .OrderBy(c => rand.Next())
                    .First())
                .Select(srv => new
                {
                    srv.Address,
                    srv.Port
                });
            
            foreach (var serviceName in lngServices)
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://{serviceName.Address}:{serviceName.Port}/api/Language");
                var response = await httpClient.SendAsync(httpRequestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode);
                }

                items.Add(await response.Content.ReadAsAsync<LanguageModel>());
            }
            return Ok(new
            {
                aggregateMachineName = Environment.MachineName,
                date = DateTime.Now.ToLongTimeString(),
                items
            });
        }
    }
}
