using Arq.Core;
using Domain.Entities;
using HostWorker.Models;
using MediatR;
using System.Net;

namespace Application.UseCases.Operation.SendToScada;

public class SendToScadaRequest : IRequest<Response<SendToScadaResponse>>
{
    public string DbChoice { get; set; }
}

public class SendToScadaHandler(
    IQuerySqlDb<ProcessOrder> processOrderQuerySqlDB,
    ICommandSqlDb<ProcessOrder> processOrderCommandSqlDB,
    IQuerySqlDb<ProcessOrderComponent> processOrderComponentQuerySqlDB,
    ICommandSqlDb<ProcessOrderComponent> processOrderComponentCommandSqlDB) :
    IRequestHandler<SendToScadaRequest, Response<SendToScadaResponse>>
{
    public async Task<Response<SendToScadaResponse>> Handle(SendToScadaRequest request, CancellationToken cancellationToken)
    {

        try
        {
            var DestinoDb = string.Empty;
            var processOrder = await processOrderQuerySqlDB.FirstOrDefaultAsync(x => x.CommStatus == 0, request.DbChoice);
            if (processOrder != null)
            {
                var components = await processOrderComponentQuerySqlDB.WhereAsync(x => x.ManufacturingOrder == processOrder.ManufacturingOrder, request.DbChoice);

                if (processOrder.DestinoRecetaDeControl == 10)
                    DestinoDb = "SapScada1";
                if (processOrder.DestinoRecetaDeControl == 20)
                    DestinoDb = "SapScada2";
                else
                {
                    return new Response<SendToScadaResponse>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new SendToScadaResponse
                        {
                            Result = false,
                            Message = "The database code is invalid"
                        }
                    };
                }

                await processOrderCommandSqlDB.AddAsync(processOrder, DestinoDb);
                foreach (var component in components)
                {
                    await processOrderComponentCommandSqlDB.AddAsync(component, DestinoDb);
                }

                processOrder.CommStatus = 2;
                await processOrderCommandSqlDB.UpdateAsync(processOrder, request.DbChoice);
            }

            return new Response<SendToScadaResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SendToScadaResponse
                {
                    Result = true,
                    Message = $"Confirmation sent to {DestinoDb}"
                }
            };
        }
        catch (Exception ex)
        {
            return new Response<SendToScadaResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendToScadaResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

}
