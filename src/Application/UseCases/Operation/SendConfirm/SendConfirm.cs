using Application.Helpers;
using Application.Interfaces;
using Arq.Core;
using Domain.Entities;
using HostWorker.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SendConfirm;

public class SendConfirmRequest : IRequest<Response<SendConfirmResponse>>
{
    public string DbChoice { get; set; }
}

public class SendConfirmHandler(
    IQuerySqlDb<ProcessOrderConfirmation> processOrderConfirmationQuerySqlDB,
    IQuerySqlDb<ProcessOrderConfirmationMaterialMovement> materialMovementQuerySqlDB,
    IQuerySqlDb<ProcessOrder> processOrderQuerySqlDB,
    IQuerySqlDb<ProcessOrderComponent> processOrderComponentQuerySqlDB,
    ICommandSqlDb<ProcessOrderConfirmation> processOrderCommandSqlDB,
    ISapOrderService sapOrderService,
    IConfiguration configuration) :
    IRequestHandler<SendConfirmRequest, Response<SendConfirmResponse>>
{
    public async Task<Response<SendConfirmResponse>> Handle(SendConfirmRequest request, CancellationToken cancellationToken)
    {

        try
        {
            var confirmation = await processOrderConfirmationQuerySqlDB.FirstOrDefaultIncludeAsync(
                nameof(ProcessOrderConfirmation.Order),
                x => x.CommStatus == 1, request.DbChoice);

            if (confirmation == null)
            {
                return new Response<SendConfirmResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SendConfirmResponse { Result = false, Message = "No confirmation found." }
                };
            }

            var movements = await materialMovementQuerySqlDB.WhereIncludeAsync(
                nameof(ProcessOrderConfirmationMaterialMovement.ProcessOrderComponent),
                x => x.ProcessOrderConfirmationId == confirmation.Id, request.DbChoice);

            if (movements == null)
            {
                return new Response<SendConfirmResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SendConfirmResponse { Result = false, Message = "No materialMovement found." }
                };
            }

            var sapRequest = SapConfirmationMapper.MapToDto(confirmation, movements);
            //sapRequest.EnteredByUser = "DSALAZAR";
            var result = await sapOrderService.SendOrderConfirmationAsync(sapRequest);
            if (result)
            {
                //confirmation.CommStatus = 2;
                //await processOrderCommandSqlDB.UpdateAsync(confirmation, request.DbChoice);
            }

            return new Response<SendConfirmResponse>
            {
                StatusCode = result ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Content = new SendConfirmResponse
                {
                    Result = result,
                    Message = result ? "Confirmation sent to SAP." : "Failed to send confirmation to SAP."
                }
            };
        }
        catch (Exception ex)
        {
            return new Response<SendConfirmResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendConfirmResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

}
