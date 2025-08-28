using Application.Helpers;
using Application.Interfaces;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SendOrder.ToDbScada;

public class SendOrderToDbScada : IRequest<Response<SendOrderToDbScadaResponse>>
{
    public string DbChoice { get; set; }
}

public class SendOrderToDbScadaHandler(
    IFileLogger logger,
    IUnitOfWork uow,
    IConfiguration configuration) :
    IRequestHandler<SendOrderToDbScada, Response<SendOrderToDbScadaResponse>>
{
    public async Task<Response<SendOrderToDbScadaResponse>> Handle(SendOrderToDbScada request, CancellationToken cancellationToken)
    {

        await logger.LogInfoAsync("Inicio de sincronización de ordenes", 
            "Metodo: SendOrderToDbScadaHandler");
        var orderCommandDbMain = uow.CommandRepository<ProcessOrder>("SapScada");
        var orderQueryDbMain = uow.QueryRepository<ProcessOrder>("SapScada");
        var orderCommandDbAux = uow.CommandRepository<ProcessOrder>(request.DbChoice);
        var orderQueryDbAux = uow.QueryRepository<ProcessOrder>(request.DbChoice);
        var componentCommandDbAux = uow.CommandRepository<ProcessOrderComponent>(request.DbChoice);

        try
        {
            await uow.BeginTransactionAsync(request.DbChoice);

            int destinoRecetaDeControl = request.DbChoice switch
            {
                "SapScada1" => 10,
                "SapScada2" => 20,
                _ => 0
            };

            // Buscar ordenes en la Bd principal
            var ordersExistDbMain = await orderQueryDbMain.WhereIncludeMultipleAsync(
                x => x.CommStatus == 1 &&
                x.DestinoRecetaDeControl == destinoRecetaDeControl,
                tracking: true,
                q => q.Include(x => x.ProcessOrderComponents));

            if (ordersExistDbMain.Count == 0)
            {
                await logger.LogInfoAsync($"No se encontraron ordenes con " +
                    $"CommStatus == 1 & destinoRecetaDeControl == " +
                    $"{destinoRecetaDeControl} en la Db principal (SapScada)",
                    "Metodo: SendOrderToDbScadaHandler");
                return new Response<SendOrderToDbScadaResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SendOrderToDbScadaResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }


            // Actualiza o inserta ordenes y componentes en la Bd de destino
            foreach (var orderNew in ordersExistDbMain)
            {
                var orderExistDbAux = await orderQueryDbAux.FirstOrDefaultIncludeMultipleAsync(
                    x => x.ManufacturingOrder == orderNew.ManufacturingOrder,
                    tracking: true,
                    q => q.Include(x => x.ProcessOrderComponents));

                if (orderExistDbAux == null)
                {
                    var newOrder = orderNew.MapTo<ProcessOrder>();
                    await orderCommandDbAux.AddAsync(newOrder);
                }
                else
                {
                    await componentCommandDbAux.DeleteRangeAsync(orderExistDbAux.ProcessOrderComponents);
                    orderNew.MapToExisting(orderExistDbAux);
                    await orderCommandDbAux.UpdateAsync(orderExistDbAux);
                }
            }

            await uow.CommitAsync(request.DbChoice);

            // Actualiza los productos que fueron ingresados en la Bd de destino
            await uow.BeginTransactionAsync("SapScada");
            foreach (var order in ordersExistDbMain)
            {
                order.CommStatus = 2;
                await orderCommandDbMain.UpdateAsync(order);
            }
            await uow.CommitAsync("SapScada");

            await logger.LogInfoAsync($"Adicion exitosa de las ordenes y componentes en la Db: " +
                $"{request.DbChoice}", "Metodo: SendOrderToDbScadaHandler");
            return new Response<SendOrderToDbScadaResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SendOrderToDbScadaResponse
                {
                    Result = true,
                    Message = string.Empty
                }
            };

        }
        catch (Exception ex)
        {
            await uow.RollbackAllAsync();
            await logger.LogErrorAsync($"Error: {ex}",
                "Metodo: SendOrderToDbScadaHandler");
            return new Response<SendOrderToDbScadaResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendOrderToDbScadaResponse
                {
                    Result = false,
                    Message = $"Error: {ex}"
                }
            };
        }

    }

}
