using Consul;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var HOSTNAME = System.Net.Dns.GetHostName();

using (var client = new ConsulClient(new ConsulClientConfiguration
       {
           Address = new Uri("http://consul:8500")
       }))
{
    await client.Agent.ServiceRegister(new AgentServiceRegistration
    {
        ID = HOSTNAME,
        Name = "csharp-service",
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