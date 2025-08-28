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
    public List<string> DbChoices { get; set; } = new();
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

        try
        {
            // Busca productos en la Db principal 
            var productsExistDbMain = await productQueryDbMain.WhereAsync(
                x => x.CommStatus == 1, 
                tracking: true);

            if (productsExistDbMain.Count == 0)
            {
                await logger.LogInfoAsync($"No se encontraron productos " +
                    $"con CommStatus == 1 en la Db principal (SapScada)",
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


            bool allSucceeded = true;

            foreach (var dbChoice in request.DbChoices)
            {
                try
                {
                    var productCommandDbAux = uow.CommandRepository<Product>(dbChoice);
                    var productQueryDbAux = uow.QueryRepository<Product>(dbChoice);

                    await uow.BeginTransactionAsync(dbChoice);

                    foreach (var productNew in productsExistDbMain)
                    {
                        var productExistDbAux = await productQueryDbAux.FirstOrDefaultAsync(
                            x => x.ProductCode == productNew.ProductCode,
                            tracking: true);

                        if (productExistDbAux == null)
                        {
                            var newProduct = productNew.MapTo<Product>();
                            await productCommandDbAux.AddAsync(newProduct);
                        }
                        else
                        {
                            productNew.MapToExisting(productExistDbAux);
                            await productCommandDbAux.UpdateAsync(productExistDbAux);
                        }
                    }

                    await uow.CommitAsync(dbChoice);

                    await logger.LogInfoAsync($"Adicion exitosa de los productos en la Db: {dbChoice}",
                        "Metodo: SyncProductHandler");
                }
                catch (Exception ex)
                {
                    allSucceeded = false;
                    await uow.RollbackAsync(dbChoice);
                    await logger.LogErrorAsync($"Error en Db: {dbChoice} -> {ex.Message}",
                        "Metodo: SyncProductHandler");
                }
            }

            // Si todas las bases fueron sincronizadas correctamente, actualizamos en la principal
            if (allSucceeded)
            {
                await uow.BeginTransactionAsync("SapScada");
                foreach (var product in productsExistDbMain)
                {
                    product.CommStatus = 2;
                    await productCommandDbMain.UpdateAsync(product);
                }
                await uow.CommitAsync("SapScada");

                await logger.LogInfoAsync("CommStatus actualizado en la Db principal (SapScada)",
                    "Metodo: SyncProductHandler");
            }

            return new Response<SyncProductResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SyncProductResponse
                {
                    Result = allSucceeded,
                    Message = allSucceeded
                        ? "Sincronización completada en todas las bases y productos actualizados en SapScada"
                        : "Sincronización parcial: algunas bases fallaron, revisar logs"
                }
            };
        }
        catch (Exception ex)
        {
            await uow.RollbackAllAsync();
            await logger.LogErrorAsync($"Error: {ex}","Metodo: SyncProductHandler");
            return new Response<SyncProductResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SyncProductResponse
                {
                    Result = false,
                    Message = $"Error: {ex}"
                }
            };
        }

    }

}
