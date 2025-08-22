using Application.Helpers;
using Application.Interfaces;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SynchronizeRecipe;

public class SyncRecipe : IRequest<Response<SyncRecipeResponse>>
{
    public string DbChoice { get; set; }
}

public class SyncRecipeHandler(
    IFileLogger logger,
    IUnitOfWork uow,
    IConfiguration configuration) :
    IRequestHandler<SyncRecipe, Response<SyncRecipeResponse>>
{
    public async Task<Response<SyncRecipeResponse>> Handle(SyncRecipe request, CancellationToken cancellationToken)
    {

        await logger.LogInfoAsync("Inicio de sincronizacion de productos", "Metodo: SyncProductHandler");

        var recipeCommandDbMain = uow.CommandRepository<Recipe>("SapScada");
        var recipeQueryDbMain = uow.QueryRepository<Recipe>("SapScada");
        var recipeCommandDbAux = uow.CommandRepository<Recipe>(request.DbChoice);
        var recipeQueryDbAux = uow.QueryRepository<Recipe>(request.DbChoice);
        var recipeBomCommandDbAux = uow.CommandRepository<RecipeBom>(request.DbChoice);

        try
        {
            await uow.BeginTransactionAsync("SapScada");
            await uow.BeginTransactionAsync(request.DbChoice);

            // Busca recetas en la Bd principal 
            var recipesExistDbMain = await recipeQueryDbMain.WhereIncludeMultipleAsync(
                x => x.CommStatus == 1,
                tracking: true,
                q => q.Include(x => x.RecipeBoms));

            if (recipesExistDbMain.Count > 0)
            {
                await logger.LogInfoAsync($"No se encontraron recetas con CommStatus == 1 en la Db: {request.DbChoice}",
                    "Metodo: SyncRecipeHandler");
                return new Response<SyncRecipeResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SyncRecipeResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

            // Actualiza o inserta las recetas en la Bd de destino que fueron encontradas en la Bd principal
            foreach (var recipeNew in recipesExistDbMain) 
            {
                var recipeExistDbAux = await recipeQueryDbAux.FirstOrDefaultAsync(
                    x => x.BillOfMaterialHeaderUuid == recipeNew.BillOfMaterialHeaderUuid, 
                    tracking: true);

                if (recipeExistDbAux == null)
                {
                    await recipeCommandDbAux.AddAsync(recipeNew);
                    await recipeBomCommandDbAux.AddRangeAsync(recipeNew.RecipeBoms);
                }
                else
                {
                    await recipeBomCommandDbAux.DeleteRangeAsync(recipeExistDbAux.RecipeBoms);
                    recipeExistDbAux = recipeNew.MapTo<Recipe>();
                    await recipeCommandDbAux.UpdateAsync(recipeExistDbAux);
                    await recipeBomCommandDbAux.AddRangeAsync(recipeNew.RecipeBoms);
                }
            }

            // Actualiza en la Bd principal las recetas que fueron ingresadas en la Bd de destino
            foreach (var recipeExist in recipesExistDbMain)
            {
                recipeExist.CommStatus = 2;
                await recipeCommandDbMain.UpdateAsync(recipeExist);
            }

            await uow.CommitAllAsync();

            await logger.LogInfoAsync($"Adicion exitosa de las recetas en la Db: {request.DbChoice}", "Metodo: SyncRecipeHandler");
            return new Response<SyncRecipeResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SyncRecipeResponse
                {
                    Result = true,
                    Message = string.Empty
                }
            };

        }
        catch (Exception ex)
        {
            await uow.RollbackAllAsync();
            await logger.LogErrorAsync($"Error: {ex.Message}", "Metodo: SyncRecipeHandler");
            return new Response<SyncRecipeResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SyncRecipeResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

}
