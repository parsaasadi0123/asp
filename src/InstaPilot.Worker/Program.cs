using InstaPilot.Infrastructure;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => options.ServiceName = "InstaPilot Worker");
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();
await builder.Build().RunAsync();
internal sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("InstaPilot Worker started.");
        while (!stoppingToken.IsCancellationRequested) await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
