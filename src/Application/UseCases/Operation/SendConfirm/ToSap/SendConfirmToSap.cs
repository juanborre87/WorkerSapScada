using Application.Helpers;
using Application.Interfaces;
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
    public bool SendWithoutMovements { get; set; }
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
            await uow.BeginTransactionAsync("SapScada");

            // Buscar confirmaciones que tengan movimiento en la Bd principal
            var confirmsExistDbMain = await confirmQueryDbMain.WhereIncludeMultipleAsync(
                    x => x.CommStatus == 1,
                    tracking: true,
                    q => q.Include(x => x.Order)
                            .ThenInclude(o => o.ProcessOrderComponents)
                          .Include(x => x.ProcessOrderConfirmationMaterialMovements));

            if (confirmsExistDbMain.Count == 0)
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

            var allSucceeded = true;

            foreach (var confirm in confirmsExistDbMain)
            {
                try
                {
                    if (request.SendWithoutMovements)
                        confirm.ProcessOrderConfirmationMaterialMovements = [];

                    var sapRequest = SapConfirmationMapper.MapToDto(confirm);
                    var result = await sapOrderService.SendOrderConfirmationAsync(sapRequest);

                    confirm.CommStatus = 2;
                    confirm.Sapresponse = result.Response;

                    await confirmCommandDbMain.UpdateAsync(confirm);

                    await logger.LogInfoAsync(
                        $"Confirmación {confirm.IdGuid} enviada correctamente a SAP y actualizada en SapScada",
                        "Metodo: SendConfirmToSapHandler");
                    await logger.LogInfoAsync(
                        $"StatusCode {(int)result.Code} / {result.Code}");
                }
                catch (Exception exItem)
                {
                    allSucceeded = false;

                    await logger.LogErrorAsync(
                        $"Error al procesar confirmación {confirm.IdGuid}: {exItem.Message}",
                        "Metodo: SendConfirmToSapHandler");
                }
            }

            await uow.CommitAllAsync();

            if (allSucceeded)
            {
                await logger.LogInfoAsync(
                    $"Todas las confirmaciones se enviaron exitosamente a SAP y fueron actualizadas en SapScada",
                    "Metodo: SendConfirmToSapHandler");

                return new Response<SendConfirmToSapResponse>
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new SendConfirmToSapResponse
                    {
                        Result = true,
                        Message = "Todas las confirmaciones fueron enviadas correctamente"
                    }
                };
            }
            else
            {
                await uow.RollbackAllAsync();
                await logger.LogInfoAsync("Algunas confirmaciones fallaron al enviarse a SAP. No se guardaron los cambios.");
                return new Response<SendConfirmToSapResponse>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new SendConfirmToSapResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

        }
        catch (Exception ex)
        {
            await uow.RollbackAllAsync();
            await logger.LogErrorAsync($"Error: {ex}",
                "Metodo: SendConfirmToSapHandler");
            return new Response<SendConfirmToSapResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendConfirmToSapResponse
                {
                    Result = false,
                    Message = $"Error: {ex}"
                }
            };
        }

    }

}
