using Application.UseCases.Operation.SendConfirm;
using Application.UseCases.Operation.SendToScada;
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

            await scopedService.SendConfirm("SapScada1");
            await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);

            await scopedService.SendConfirm("SapScada1");
            await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);

            await scopedService.SendToScada("SapScadaMain");
            await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);
        }
    }
}

public interface IScopedService
{
    Task<bool> SendConfirm(string DbChoice);

    Task<bool> SendToScada(string DbChoice);
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

    public async Task<bool> SendConfirm(string DbChoice)
    {
        var result = await _mediator.Send(new SendConfirmRequest() { DbChoice = DbChoice });
        return result.Content.Result;
    }

    public async Task<bool> SendToScada(string DbChoice)
    {
        var result = await _mediator.Send(new SendToScadaRequest() { DbChoice = DbChoice });
        return result.Content.Result;
    }
}
