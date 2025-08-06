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
    IQuerySqlDb<ProcessOrderComponent> processOrderComponentQuerySqlDB,
    IQuerySqlDb<Product> productQuerySqlDB,
    ICommandSqlDb<Product> productCommandSqlDb,
    ICommandSqlDb<ProcessOrder> processOrderCommandSqlDB,
    ICommandSqlDb<ProcessOrderComponent> processOrderComponentCommandSqlDB) :
    IRequestHandler<SendToScadaRequest, Response<SendToScadaResponse>>
{
    public async Task<Response<SendToScadaResponse>> Handle(SendToScadaRequest request, CancellationToken cancellationToken)
    {

        try
        {
            var targetDb = string.Empty;
            var processOrder = await processOrderQuerySqlDB.FirstOrDefaultAsync(x => x.CommStatus == 0, request.DbChoice);
            if (processOrder != null)
            {
                var components = await processOrderComponentQuerySqlDB.WhereAsync(x => x.ManufacturingOrder == processOrder.ManufacturingOrder, request.DbChoice);

                if (processOrder.DestinoRecetaDeControl == 10)
                {
                    targetDb = "SapScada1";
                    var result = await SyncProductsAsync(request.DbChoice, targetDb);
                }

                if (processOrder.DestinoRecetaDeControl == 20)
                {
                    targetDb = "SapScada2";
                    var result = await SyncProductsAsync(request.DbChoice, targetDb);
                }
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

                await processOrderCommandSqlDB.AddAsync(processOrder, targetDb);
                foreach (var component in components)
                {
                    await processOrderComponentCommandSqlDB.AddAsync(component, targetDb);
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
                    Message = $"Confirmation sent to {targetDb}"
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

    public async Task<int> SyncProductsAsync(string SourceDb, string TargetDb)
    {
        var sourceProducts = await productQuerySqlDB.ListAllAsync(SourceDb);
        var targetProducts = await productQuerySqlDB.ListAllAsync(TargetDb);

        // Aquí asumimos que se comparan por el campo "Codigo"
        var targetCodes = new HashSet<string>(sourceProducts.Select(m => m.ProductCode));

        // Filtrar los materiales faltantes
        var missingProducts = sourceProducts
            .Where(m => !targetCodes.Contains(m.ProductCode))
            .ToList();

        // Insertar los materiales faltantes en la tabla de destino
        foreach (var product in missingProducts)
        {
            await productCommandSqlDb.AddAsync(product, TargetDb);
        }

        // Retornar la cantidad de materiales insertados
        return missingProducts.Count;
    }

}
