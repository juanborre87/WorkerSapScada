using Application.Helpers;
using Application.Interfaces;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SendConfirm.ToSap;

public class SendConfirmToSap : IRequest<Response<SendConfirmToSapResponse>>
{
    public string DbChoice { get; set; }
}

public class SendConfirmToSapHandler(
    IUnitOfWork uow,
    IFileLogger logger,
    IQuerySqlDb<ProcessOrderConfirmation> processOrderConfirmationQuerySqlDB,
    IQuerySqlDb<ProcessOrderConfirmationMaterialMovement> materialMovementQuerySqlDB,
    IQuerySqlDb<ProcessOrder> processOrderQuerySqlDB,
    ICommandSqlDb<ProcessOrder> processOrderCommandSqlDB,
    ICommandSqlDb<ProcessOrderConfirmation> processOrderConfirmationCommandSqlDB,
    ISapOrderService sapOrderService,
    IConfiguration configuration) :
    IRequestHandler<SendConfirmToSap, Response<SendConfirmToSapResponse>>
{
    public async Task<Response<SendConfirmToSapResponse>> Handle(SendConfirmToSap request, CancellationToken cancellationToken)
    {

        try
        {
            // Busca confirmacion en la Bd principal
            var confirmation = await processOrderConfirmationQuerySqlDB
                .FirstOrDefaultAsync(x => x.CommStatus == 1, request.DbChoice, false);

            if (confirmation == null)
            {
                await logger.LogInfoAsync($"No se encontró confirmación en la Db: {request.DbChoice}",
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

            // Busca la orden de la confirmacion en la Bd principal
            var processOrder = await processOrderQuerySqlDB
                .FirstOrDefaultAsync(x => x.ManufacturingOrder == confirmation.OrderId, request.DbChoice, false);
            confirmation.Order = processOrder;

            // Busca los movimientos de material en la Bd principal
            var movements = await materialMovementQuerySqlDB
                .WhereIncludeAsync(nameof(ProcessOrderConfirmationMaterialMovement.ProcessOrderComponent),
                x => x.ProcessOrderConfirmationId == confirmation.Id, request.DbChoice, false);

            if (movements.Count == 0)
            {
                await logger.LogInfoAsync($"No se encontró movimiento de material en la confirmacion: {confirmation.Id}",
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
            var sapRequest = SapConfirmationMapper.MapToDto(confirmation, movements);
            var result = await sapOrderService.SendOrderConfirmationAsync(sapRequest);
            await logger.LogInfoAsync("Confirmacion enviada a SAP", "Metodo: SendOrderConfirmationAsync");
            if (result.Result)
            {
                try
                {
                    confirmation.CommStatus = 2;
                    await processOrderConfirmationCommandSqlDB.UpdateAsync(confirmation, request.DbChoice);
                    await logger.LogInfoAsync("Actualizacion exitosa de la confirmacion CommStatus = 2 en la Db: SapScadaMain",
                        "Metodo: SendConfirmToSapHandler");

                }
                catch (Exception ex)
                {
                    await logger.LogErrorAsync($"Error al actualizar la confirmacion en la Db {request.DbChoice}: {ex.Message}",
                        "Metodo: SendConfirmToSapHandler");
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

            return new Response<SendConfirmToSapResponse>
            {
                StatusCode = result.Result ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Content = new SendConfirmToSapResponse
                {
                    Result = result.Result,
                    Message = string.Empty
                }
            };
        }
        catch (Exception ex)
        {
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
