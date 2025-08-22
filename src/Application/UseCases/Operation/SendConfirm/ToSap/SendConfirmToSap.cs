using Application.Helpers;
using Application.Interfaces;
using Application.UseCases.Operation.SendOrder.ToDbScada;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SendConfirm.ToSap;

public class SendConfirmToSap : IRequest<Response<SendConfirmToSapResponse>>
{
    public string DbChoice { get; set; }
}

public class SendConfirmToSapHandler(
    IFileLogger logger,
    IUnitOfWork uow,
    ISendConfirmSapService sapOrderService,
    IConfiguration configuration) :
    IRequestHandler<SendConfirmToSap, Response<SendConfirmToSapResponse>>
{
    public async Task<Response<SendConfirmToSapResponse>> Handle(SendConfirmToSap request, CancellationToken cancellationToken)
    {
        await logger.LogInfoAsync("Inicio de sincronización de órdenes",
            "Metodo: SendConfirmToSapHandler");
        var orderCommandDbMain = uow.CommandRepository<ProcessOrder>("SapScada");
        var orderQueryDbMain = uow.QueryRepository<ProcessOrder>("SapScada");
        var confirmQueryDbMain = uow.QueryRepository<ProcessOrderConfirmation>("SapScada");
        var confirmCommandDbMain = uow.CommandRepository<ProcessOrderConfirmation>("SapScada");

        try
        {
            // Buscar confirmaciones que tengan movimiento en la Bd principal
            var confirmsExistDbMain = await confirmQueryDbMain.WhereIncludeMultipleAsync(
                x => x.CommStatus == 1 && x.ProcessOrderConfirmationMaterialMovements.Any(),
                    tracking: true,
                    q => q.Include(x => x.Order)
                            .ThenInclude(o => o.ProcessOrderComponents)
                          .Include(x => x.ProcessOrderConfirmationMaterialMovements));

            if (confirmsExistDbMain.Count > 0)
            {
                await logger.LogInfoAsync($"No se encontraron confirmaciones con CommStatus == 1",
                    "Metodo: SendConfirmToSapHandler");
                return new Response<SendConfirmToSapResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SendConfirmToSapResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

            // Mapea los valores necesarios para enviarlos a Sap
            foreach (var confirm in confirmsExistDbMain)
            {
                var sapRequest = SapConfirmationMapper.MapToDto(confirm);
                var result = await sapOrderService.SendOrderConfirmationAsync(sapRequest);
                confirm.CommStatus = 2;
                confirm.Sapresponse = result.Response;
                await confirmCommandDbMain.UpdateAsync(confirm);
                await logger.LogInfoAsync($"Actualizacion exitosa de la confirmacion {confirm.IdGuid} en la Db: SapScadaMain",
                    "Metodo: SendConfirmToSapHandler");
            }

            await uow.CommitAllAsync();

            await logger.LogInfoAsync($"Adicion exitosa de las ordenes y componentes en la Db: " +
                $"{request.DbChoice}", "Metodo: SyncProductHandler");
            return new Response<SendConfirmToSapResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SendConfirmToSapResponse
                {
                    Result = true,
                    Message = string.Empty
                }
            };

        }
        catch (Exception ex)
        {
            await uow.RollbackAllAsync();
            await logger.LogErrorAsync($"Error: {ex.Message}",
                "Metodo: SendConfirmToSapHandler");
            return new Response<SendConfirmToSapResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendConfirmToSapResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

}
