using Application.Helpers;
using Application.Interfaces;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SendConfirm.ToDbMain;

public class SendConfirmToDbMain : IRequest<Response<SendConfirmToDbMainResponse>>
{
    public string DbChoice { get; set; }
}

public class SendConfirmToDbMainHandler(
    IFileLogger logger,
    IUnitOfWork uow,
    IConfiguration configuration) :
    IRequestHandler<SendConfirmToDbMain, Response<SendConfirmToDbMainResponse>>
{
    public async Task<Response<SendConfirmToDbMainResponse>> Handle(SendConfirmToDbMain request, CancellationToken cancellationToken)
    {

        await logger.LogInfoAsync("Inicio de sincronización de órdenes",
            "Metodo: SendConfirmToDbMainHandler");
        var confirmCommandDbMain = uow.CommandRepository<ProcessOrderConfirmation>("SapScada");
        var confirmQueryDbAux = uow.QueryRepository<ProcessOrderConfirmation>(request.DbChoice);
        var confirmAuxCommandDbAux = uow.CommandRepository<ProcessOrderConfirmation>(request.DbChoice);
        var movementCommandDbMain = uow.CommandRepository<ProcessOrderConfirmationMaterialMovement>("SapScada");

        try
        {
            await uow.BeginTransactionAsync("SapScada");
            await uow.BeginTransactionAsync(request.DbChoice);

            // Buscar confirmaciones en la Bd aux
            var confirmsExistDbAux = await confirmQueryDbAux.WhereIncludeMultipleAsync(
                x => x.CommStatus == 1,
                tracking: true,
                q => q.Include(x => x.ProcessOrderConfirmationMaterialMovements));

            if (confirmsExistDbAux.Count == 0)
            {
                await logger.LogInfoAsync($"No se encontraron movimientos con CommStatus == 1",
                    "Metodo: SendConfirmToDbMainHandler");
                return new Response<SendConfirmToDbMainResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SendConfirmToDbMainResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

            // Insertar confirmaciones y movimientos en la Bd principal
            foreach (var confirmAux in confirmsExistDbAux)
            {
                var newConfirm = confirmAux.MapTo<ProcessOrderConfirmation>();
                await confirmCommandDbMain.AddAsync(newConfirm);
            }

            // Actualiza las confirmaciones que fueron sincronizadas
            foreach (var confirm in confirmsExistDbAux)
            {
                confirm.CommStatus = 2;
                await confirmAuxCommandDbAux.UpdateAsync(confirm);
            }

            await uow.CommitAllAsync();

            await logger.LogInfoAsync($"Adicion exitosa de las confirmaciones y movimientos en la Db: " +
                $"{request.DbChoice}", "Metodo: SendConfirmToDbMainHandler");
            return new Response<SendConfirmToDbMainResponse>
            {
                StatusCode = HttpStatusCode.OK,
                Content = new SendConfirmToDbMainResponse
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
                "Metodo: SendConfirmToDbMainHandler");
            return new Response<SendConfirmToDbMainResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendConfirmToDbMainResponse
                {
                    Result = false,
                    Message = $"Error: {ex}"
                }
            };
        }

    }

}
