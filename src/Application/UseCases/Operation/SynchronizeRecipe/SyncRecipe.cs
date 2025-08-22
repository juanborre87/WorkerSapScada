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

            var targetDb = string.Empty;
            // Busca recetas en la Bd principal 
            var recipeExistDbMain = await recipeQueryDbMain.WhereIncludeMultipleAsync(
                x => x.CommStatus == 1,
                tracking: true,
                q => q.Include(x => x.RecipeBoms));
            if (recipeExistDbMain.Count > 0)
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

            var sourceRecipes = await recipeQueryDbMain.ListAllAsync();
            var targetRecipes = await recipeQueryDbAux.ListAllAsync();

            // Crear un conjunto con los códigos de recetas ya existentes en la Bd destino
            var targetCodes = new HashSet<Guid>(targetRecipes.Select(p => p.BillOfMaterialHeaderUuid));

            // Filtrar las recetas que están en origen pero no en destino
            var missingRecipes = sourceRecipes
                .Where(p => !targetCodes.Contains(p.BillOfMaterialHeaderUuid))
                .ToList();

            // Insertar las recetas y sus items faltantes en la Bd destino
            foreach (var recipe in missingRecipes)
            {
                await recipeCommandDbAux.AddAsync(recipe);
                await recipeBomCommandDbAux.AddRangeAsync(recipe.RecipeBoms);
            }

            // Actualiza las recetas que fueron ingresadas en la Bd de destino
            foreach (var confirm in recipeExistDbMain)
            {
                confirm.CommStatus = 2;
                await recipeCommandDbMain.UpdateAsync(confirm);
            }

            await uow.CommitAllAsync();

            await logger.LogInfoAsync($"Adicion exitosa de las recetas en la Db: {targetDb}", "Metodo: SyncRecipeHandler");
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
