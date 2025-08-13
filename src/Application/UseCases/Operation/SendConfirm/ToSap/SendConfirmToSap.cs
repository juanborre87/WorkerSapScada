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
    ISapOrderService sapOrderService,
    IConfiguration configuration) :
    IRequestHandler<SendConfirmToSap, Response<SendConfirmToSapResponse>>
{
    public async Task<Response<SendConfirmToSapResponse>> Handle(SendConfirmToSap request, CancellationToken cancellationToken)
    {

        try
        {
            var confirmation = await processOrderConfirmationQuerySqlDB
                .FirstOrDefaultIncludeAsync(nameof(ProcessOrderConfirmation.Order),
                x => x.CommStatus == 1, request.DbChoice, true);

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

            var movements = await materialMovementQuerySqlDB
                .WhereIncludeAsync(nameof(ProcessOrderConfirmationMaterialMovement.ProcessOrderComponent),
                x => x.ProcessOrderConfirmationId == confirmation.Id, request.DbChoice, false);

            if (movements == null)
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

            var sapRequest = SapConfirmationMapper.MapToDto(confirmation, movements);
            //sapRequest.EnteredByUser = "DSALAZAR";
            var result = await sapOrderService.SendOrderConfirmationAsync(sapRequest);
            if (result.Result)
            {
                await uow.BeginTransactionAsync(request.DbChoice);
                try
                {
                    var processOrder = await processOrderQuerySqlDB.FirstOrDefaultAsync(x => x.Id == confirmation.Order.Id, request.DbChoice, true);
                    processOrder.CommStatus = 2;
                    await processOrderCommandSqlDB.UpdateToTransactionAsync(processOrder, request.DbChoice);
                    //confirmation.Sapresponse = result.Response;
                    //await processOrderConfirmationCommandSqlDB.UpdateToTransactionAsync(confirmation, request.DbChoice);

                    await uow.CommitAsync();

                    await logger.LogInfoAsync("Actualizacion exitosa de la orden CommStatus = 2",
                        "Metodo: SendConfirmToSapHandler");

                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    await logger.LogErrorAsync($"Error al actualizar la orden: {ex.Message}",
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
                    Message = result.Result ? "Confirmacion enviada a SAP" : "Envío fallido de la confirmacion a SAP."
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
