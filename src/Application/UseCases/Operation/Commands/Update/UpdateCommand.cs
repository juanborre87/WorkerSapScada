using Application.Interfaces;
using Application.UseCases.Operation.Commands.Create;
using Domain.Entities;
using Domain.Models;
using HostWorker.Models;
using MediatR;
using System.Net;

namespace Application.UseCases.Operation.Commands.Update;

public class UpdateCommand : IRequest<Response<UpdateResponse>>
{
}

public class UpdateCommandHandler(
    IQuerySqlDB<ProcessOrderConfirmation> processOrderConfirmationQuerySqlDB,
    IQuerySqlDB<ProcessOrder> processOrderQuerySqlDB,
    IQuerySqlDB<ProcessOrderComponent> processOrderComponentQuerySqlDB,
    ICommandSqlDB<ProcessOrderConfirmation> processOrderCommandSqlDB,
    ISapOrderService sapOrderService) :
    IRequestHandler<UpdateCommand, Response<UpdateResponse>>
{
    public async Task<Response<UpdateResponse>> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        if (request != null)
        {
            return new Response<UpdateResponse>
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new UpdateResponse
                {
                    Result = true
                }
            };

        }

        return new Response<UpdateResponse>
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new UpdateResponse
            {
                Result = false
            }
        };

    }
}
