using Application.Interfaces;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SendOrder.ToDbScada;

public class SendOrderToDbScada : IRequest<Response<SendOrderToDbScadaResponse>>
{
    public string DbChoice { get; set; }
}

public class SendOrderToDbScadaHandler(
    IUnitOfWork uow,
    IFileLogger logger,
    IQuerySqlDb<ProcessOrder> processOrderQuerySqlDB,
    IQuerySqlDb<ProcessOrderComponent> processOrderComponentQuerySqlDB,
    IQuerySqlDb<Product> productQuerySqlDB,
    ICommandSqlDb<Product> productCommandSqlDb,
    ICommandSqlDb<ProcessOrder> processOrderCommandSqlDB,
    ICommandSqlDb<ProcessOrderComponent> processOrderComponentCommandSqlDB,
    IConfiguration configuration) :
    IRequestHandler<SendOrderToDbScada, Response<SendOrderToDbScadaResponse>>
{
    public async Task<Response<SendOrderToDbScadaResponse>> Handle(SendOrderToDbScada request, CancellationToken cancellationToken)
    {

        try
        {
            var targetDb = string.Empty;
            var processOrder = await processOrderQuerySqlDB
                .FirstOrDefaultAsync(x => x.CommStatus == 1, request.DbChoice, false);

            if (processOrder == null)
            {
                await logger.LogInfoAsync($"No se encontró orden con CommStatus == 1 en la Db: {request.DbChoice}",
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

            var components = await processOrderComponentQuerySqlDB
                    .WhereAsync(x => x.ManufacturingOrder == processOrder.ManufacturingOrder, request.DbChoice, false);

            if (processOrder.DestinoRecetaDeControl == 10)
            {
                targetDb = "SapScada1";
                var result = await SynchronizeProductsAsync(request.DbChoice, targetDb);
            }

            if (processOrder.DestinoRecetaDeControl == 20)
            {
                targetDb = "SapScada2";
                var result = await SynchronizeProductsAsync(request.DbChoice, targetDb);
            }
            else
            {
                await logger.LogErrorAsync($"El DestinoRecetaDeControl es inválido debe ser: 10 o 20 {processOrder.DestinoRecetaDeControl}",
                    "Metodo: SendOrderToDbScadaHandler");
                return new Response<SendOrderToDbScadaResponse>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new SendOrderToDbScadaResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

            await uow.BeginTransactionAsync(targetDb);

            try
            {
                await processOrderCommandSqlDB.AddToTransactionAsync(processOrder, targetDb);
                await processOrderComponentCommandSqlDB.AddRangeToTransactionAsync(components, targetDb);

                await logger.LogInfoAsync("Adicion exitosa de la orden y los componentes",
                    "Metodo: SendOrderToDbScadaHandler");
                await uow.CommitAsync();
                uow.Dispose();

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
                await uow.RollbackAsync();
                await logger.LogErrorAsync($"Error al guardar la orden: {ex.Message}",
                    "Metodo: SendOrderToDbScadaHandler");
                return new Response<SendOrderToDbScadaResponse>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new SendOrderToDbScadaResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error: {ex.Message}",
                "Metodo: SendOrderToDbScadaHandler");
            return new Response<SendOrderToDbScadaResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendOrderToDbScadaResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

    public async Task<int> SynchronizeProductsAsync(string SourceDb, string TargetDb)
    {
        var sourceProducts = await productQuerySqlDB.ListAllAsync(SourceDb);
        var targetProducts = await productQuerySqlDB.ListAllAsync(TargetDb);

        // Crear un conjunto con los códigos de producto ya existentes en la base de datos destino
        var targetCodes = new HashSet<string>(targetProducts.Select(p => p.ProductCode));

        // Filtrar los productos que están en origen pero no en destino
        var missingProducts = sourceProducts
            .Where(p => !targetCodes.Contains(p.ProductCode))
            .ToList();

        // Insertar los productos faltantes en la base de datos destino
        await productCommandSqlDb.AddRangeAsync(missingProducts, TargetDb);

        // Retornar la cantidad de productos insertados
        return missingProducts.Count;
    }

}
