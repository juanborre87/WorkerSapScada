using Application.UseCases.Operation.Commands.Create;
using Application.UseCases.Operation.Commands.Update;
using HostWorker.Models;
using MediatR;

namespace WorkerSapScada;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string delayTimeString = _configuration.GetValue<string>("Delay:TimeMinutes");
        int delayTime = Convert.ToInt32(delayTimeString);

        using var scope = _scopeFactory.CreateScope();
        while (!stoppingToken.IsCancellationRequested)
        {
            var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
            await scopedService.Update();
            await Task.Delay(TimeSpan.FromMinutes(delayTime), stoppingToken);
        }
    }
}

public interface IScopedService
{
    Task<bool> Create();

    Task<bool> Update();
}

public class ScopedService : BaseApiController, IScopedService
{
    private readonly ILogger<ScopedService> _logger;
    private readonly ISender _mediator;

    public ScopedService(ILogger<ScopedService> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<bool> Create()
    {
        var result = await _mediator.Send(new CreateCommand() { });
        return result.Content.Result;
    }

    public async Task<bool> Update()
    {
        var result = await _mediator.Send(new UpdateCommand() { });
        return result.Content.Result;
    }
}
