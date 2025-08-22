using Application.UseCases.Operation.SendConfirm.ToDbMain;
using Application.UseCases.Operation.SendConfirm.ToSap;
using Application.UseCases.Operation.SendOrder.ToDbScada;
using Application.UseCases.Operation.SynchronizeProduct;
using Application.UseCases.Operation.SynchronizeRecipe;
using Arq.Host;
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

            try
            {
                await scopedService.SyncProduct("SapScada1");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);
                await scopedService.SyncProduct("SapScada2");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);


                await scopedService.SyncRecipe("SapScada1");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);
                await scopedService.SyncRecipe("SapScada2");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);


                await scopedService.OrderToDbScada("SapScada1");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);
                await scopedService.OrderToDbScada("SapScada2");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);


                await scopedService.ConfirmToDbMain("SapScada1");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);
                await scopedService.ConfirmToDbMain("SapScada2");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);


                await scopedService.ConfirmToSap("SapScadaMain");
                await Task.Delay(TimeSpan.FromSeconds(delayTime), stoppingToken);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

public interface IScopedService
{
    /// <summary>
    /// Metodo que sincroniza los productos en la Bd indicada
    /// </summary>
    /// <param name="DbChoice">Bd de la cual se va a enviar el registro</param>
    /// <returns></returns>
    Task<bool> SyncProduct(string DbChoice);

    /// <summary>
    /// Metodo que envía la orden a las Bd del Scada
    /// </summary>
    /// <param name="DbChoice">Bd de la cual se va a enviar el registro</param>
    /// <returns></returns>
    Task<bool> SyncRecipe(string DbChoice);

    /// <summary>
    /// Metodo que envía la orden a las Bd del Scada
    /// </summary>
    /// <param name="DbChoice">Bd de la cual se va a enviar el registro</param>
    /// <returns></returns>
    Task<bool> OrderToDbScada(string DbChoice);

    /// <summary>
    /// Metodo que envía la confirmacion a la Bd principal
    /// </summary>
    /// <param name="DbChoice">Bd de la cual se va a enviar el registro</param>
    /// <returns></returns>
    Task<bool> ConfirmToDbMain(string DbChoice);

    /// <summary>
    /// Metodo que envía la confirmacion a Sap
    /// </summary>
    /// <param name="DbChoice">Bd de la cual se va a enviar el registro</param>
    /// <returns></returns>
    Task<bool> ConfirmToSap(string DbChoice);
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

    public async Task<bool> OrderToDbScada(string DbChoice)
    {
        var result = await _mediator.Send(new SendOrderToDbScada() { DbChoice = DbChoice });
        return result.Content.Result;
    }

    public async Task<bool> ConfirmToDbMain(string DbChoice)
    {
        var result = await _mediator.Send(new SendConfirmToDbMain() { DbChoice = DbChoice });
        return result.Content.Result;
    }

    public async Task<bool> ConfirmToSap(string DbChoice)
    {
        var result = await _mediator.Send(new SendConfirmToSap() { DbChoice = DbChoice });
        return result.Content.Result;
    }

    public async Task<bool> SyncProduct(string DbChoice)
    {
        var result = await _mediator.Send(new SyncProduct() { DbChoice = DbChoice });
        return result.Content.Result;
    }

    public async Task<bool> SyncRecipe(string DbChoice)
    {
        var result = await _mediator.Send(new SyncRecipe() { DbChoice = DbChoice });
        return result.Content.Result;
    }
}
