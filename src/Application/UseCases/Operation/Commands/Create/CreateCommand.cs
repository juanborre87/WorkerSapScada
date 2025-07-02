using Application.Helpers;
using Application.Interfaces;
using Domain.Entities;
using HostWorker.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.Commands.Create;

public class CreateCommand : IRequest<Response<CreateResponse>>
{
}

public class CreateCommandHandler(
    IQuerySqlDB<ProcessOrderConfirmation> processOrderConfirmationQuerySqlDB,
    IQuerySqlDB<ProcessOrderConfirmationMaterialMovement> materialMovementQuerySqlDB,
    IQuerySqlDB<ProcessOrder> processOrderQuerySqlDB,
    IQuerySqlDB<ProcessOrderComponent> processOrderComponentQuerySqlDB,
    ICommandSqlDB<ProcessOrderConfirmation> processOrderCommandSqlDB,
    ISapOrderService sapOrderService,
    IConfiguration configuration) :
    IRequestHandler<CreateCommand, Response<CreateResponse>>
{
    public async Task<Response<CreateResponse>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {

        try
        {
            var confirmation = await processOrderConfirmationQuerySqlDB.FirstOrDefaultAsync(x => x.CommStatus == 1);

            if (confirmation == null)
            {
                return new Response<CreateResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new CreateResponse { Result = false, Message = "No confirmation found." }
                };
            }

            var materialMovement = await materialMovementQuerySqlDB.WhereAsync(x => x.ProcessOrderConfirmationId == confirmation.Id);

            if (materialMovement == null)
            {
                return new Response<CreateResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new CreateResponse { Result = false, Message = "No materialMovement found." }
                };
            }

            var sapRequest = SapConfirmationMapper.MapToDto(confirmation, materialMovement);
            var result = await sapOrderService.SendOrderConfirmationAsync(sapRequest);


            confirmation.CommStatus = 2;
            await processOrderCommandSqlDB.UpdateAsync(confirmation);

            return new Response<CreateResponse>
            {
                StatusCode = result ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Content = new CreateResponse
                {
                    Result = result,
                    Message = result ? "Confirmation sent to SAP." : "Failed to send confirmation to SAP."
                }
            };
        }
        catch (Exception ex)
        {
            return new Response<CreateResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new CreateResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

}
