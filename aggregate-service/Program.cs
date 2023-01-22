using System.Threading.RateLimiting;
using Consul;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var slidingPolicyName = "slidingPolicy";
builder.Services.AddRateLimiter(limeterOptions =>
{
    limeterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    limeterOptions.AddSlidingWindowLimiter(policyName: slidingPolicyName, options =>
    {
        options.PermitLimit = 4;
        options.SegmentsPerWindow = 2;
        options.Window = TimeSpan.FromSeconds(60);
    });
});

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers().RequireRateLimiting(slidingPolicyName);

var HOSTNAME = System.Net.Dns.GetHostName();

using (var client = new ConsulClient(new ConsulClientConfiguration
       {
           Address = new Uri("http://consul:8500")
       }))
{
    await client.Agent.ServiceRegister(new AgentServiceRegistration
    {
        ID = HOSTNAME,
        Name = "aggregate-service",
        Address = HOSTNAME,
        Port = 80,
        Check = new AgentServiceCheck
        {
            Interval = TimeSpan.FromSeconds(10),
            Timeout = TimeSpan.FromSeconds(1),
            HTTP = $"http://{HOSTNAME}:80/health-check",
        },
  });
}


app.Run();