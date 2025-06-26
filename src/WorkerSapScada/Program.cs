using Application.Extensions;
using Infrastructure.Extensions;
using WorkerSapScada;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables()
       .AddCommandLine(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();
