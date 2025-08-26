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
    public List<string> DbChoices { get; set; } = [];
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

        try
        {
            // Busca recetas en la Bd principal 
            var recipesExistDbMain = await recipeQueryDbMain.WhereIncludeMultipleAsync(
                x => x.CommStatus == 1,
                tracking: true,
                q => q.Include(x => x.RecipeBoms));

            if (recipesExistDbMain.Count > 0)
            {
                await logger.LogInfoAsync($"No se encontraron recetas con CommStatus == 1 en la Db principal (SapScada)",
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


            bool allSucceeded = true;

            foreach (var dbChoice in request.DbChoices)
            {
                try
                {
                    var recipeCommandDbAux = uow.CommandRepository<Recipe>(dbChoice);
                    var recipeQueryDbAux = uow.QueryRepository<Recipe>(dbChoice);
                    var recipeBomCommandDbAux = uow.CommandRepository<RecipeBom>(dbChoice);

                    await uow.BeginTransactionAsync("SapScada");
                    await uow.BeginTransactionAsync(dbChoice);

                    foreach (var recipeNew in recipesExistDbMain)
                    {
                        var recipeExistDbAux = await recipeQueryDbAux.FirstOrDefaultAsync(
                            x => x.BillOfMaterialHeaderUuid == recipeNew.BillOfMaterialHeaderUuid,
                            tracking: true);

                        if (recipeExistDbAux == null)
                        {
                            var newRecipe = recipeNew.MapTo<Recipe>();
                            await recipeCommandDbAux.AddAsync(newRecipe);
                            var newRecipeBoms = recipeNew.RecipeBoms
                                .Select(c => c.MapTo<RecipeBom>())
                                .ToList();
                            await recipeBomCommandDbAux.AddRangeAsync(newRecipeBoms);
                        }
                        else
                        {
                            await recipeBomCommandDbAux.DeleteRangeAsync(recipeExistDbAux.RecipeBoms);
                            recipeExistDbAux = recipeNew.MapTo<Recipe>();
                            await recipeCommandDbAux.UpdateAsync(recipeExistDbAux);
                            var newRecipeBoms = recipeNew.RecipeBoms
                                .Select(c => c.MapTo<RecipeBom>())
                                .ToList();
                            await recipeBomCommandDbAux.AddRangeAsync(newRecipeBoms);
                        }
                    }

                    await uow.CommitAllAsync();

                    await logger.LogInfoAsync($"Adición exitosa de las recetas en la Db: {dbChoice}",
                        "Metodo: SyncRecipeHandler");
                }
                catch (Exception ex)
                {
                    allSucceeded = false;
                    await uow.RollbackAsync(dbChoice);
                    await logger.LogErrorAsync($"Error en Db: {dbChoice} -> {ex.Message}",
                        "Metodo: SyncRecipeHandler");
                }
            }

            // Si todas las bases fueron sincronizadas correctamente, actualizamos en la principal
            if (allSucceeded)
            {
                foreach (var recipeExist in recipesExistDbMain)
                {
                    recipeExist.CommStatus = 2;
                    await recipeCommandDbMain.UpdateAsync(recipeExist);
                }

                await uow.CommitAllAsync();

                await logger.LogInfoAsync("CommStatus actualizado en la Db principal (SapScada)",
                    "Metodo: SyncRecipeHandler");
            }

            return new Response<SyncRecipeResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SyncRecipeResponse
                {
                    Result = allSucceeded,
                    Message = allSucceeded
                        ? "Sincronización completada en todas las bases y recetas actualizadas en SapScada"
                        : "Sincronización parcial: algunas bases fallaron, revisar logs"
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
