using Application.Helpers;
using Application.Interfaces;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SynchronizeProduct;

public class SyncProduct : IRequest<Response<SyncProductResponse>>
{
    public string DbChoice { get; set; }
}

public class SyncProductHandler(
    IFileLogger logger,
    IUnitOfWork uow,
    IConfiguration configuration) :
    IRequestHandler<SyncProduct, Response<SyncProductResponse>>
{
    public async Task<Response<SyncProductResponse>> Handle(SyncProduct request, CancellationToken cancellationToken)
    {

        await logger.LogInfoAsync("Inicio de sincronizacion de productos", 
            "Metodo: SyncProductHandler");
        var productCommandDbMain = uow.CommandRepository<Product>("SapScada");
        var productQueryDbMain = uow.QueryRepository<Product>("SapScada");
        var productCommandDbAux = uow.CommandRepository<Product>(request.DbChoice);
        var productQueryDbAux = uow.QueryRepository<Product>(request.DbChoice);

        try
        {
            await uow.BeginTransactionAsync("SapScada");
            await uow.BeginTransactionAsync(request.DbChoice);

            // Busca productos en la Bd principal 
            var productsExistDbMain = await productQueryDbMain.WhereAsync(
                x => x.CommStatus == 1, 
                tracking:true);

            if (productsExistDbMain.Count > 0)
            {
                await logger.LogInfoAsync($"No se encontraron productos " +
                    $"con CommStatus == 1 en la Db: {request.DbChoice}",
                    "Metodo: SyncProductHandler");
                return new Response<SyncProductResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SyncProductResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

            // Actualiza o inserta los productos en la Bd de destino que fueron encontradas en la Bd principal
            foreach (var productNew in productsExistDbMain)
            {
                var productExistDbAux = await productQueryDbAux.FirstOrDefaultAsync(
                    x => x.ProductCode == productNew.ProductCode,
                    tracking: true);

                if (productExistDbAux == null)
                {
                    await productCommandDbAux.AddAsync(productNew);
                }
                else
                {
                    productExistDbAux = productNew.MapTo<Product>();
                    await productCommandDbAux.UpdateAsync(productExistDbAux);
                }
            }

            // Actualiza los productos que fueron ingresados en la Bd de destino
            foreach (var product in productsExistDbMain)
            {
                product.CommStatus = 2;
                await productCommandDbMain.UpdateAsync(product);
            }

            await uow.CommitAllAsync();

            await logger.LogInfoAsync($"Adicion exitosa de los productos en la Db: " +
                $"{request.DbChoice}", "Metodo: SyncProductHandler");
            return new Response<SyncProductResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SyncProductResponse
                {
                    Result = true,
                    Message = string.Empty
                }
            };

        }
        catch (Exception ex)
        {
            await uow.RollbackAllAsync();
            await logger.LogErrorAsync($"Error: {ex.Message}","Metodo: SyncProductHandler");
            return new Response<SyncProductResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SyncProductResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

}
