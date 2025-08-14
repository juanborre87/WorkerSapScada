using Application.Helpers;
using Application.Interfaces;
using Arq.Core;
using Arq.Host;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.SendConfirm.ToDbMain;

public class SendConfirmToDbMain : IRequest<Response<SendConfirmToDbMainResponse>>
{
    public string DbChoice { get; set; }
}

public class SendConfirmToDbMainHandler(
    IUnitOfWork uow,
    IFileLogger logger,
    IQuerySqlDb<ProcessOrderConfirmation> processOrderConfirmationQuerySqlDB,
    IQuerySqlDb<ProcessOrderConfirmationMaterialMovement> materialMovementQuerySqlDB,
    ICommandSqlDb<ProcessOrderConfirmation> processOrderConfirmationCommandSqlDB,
    ICommandSqlDb<ProcessOrderConfirmationMaterialMovement> materialMovementCommandSqlDB,
    IConfiguration configuration) :
    IRequestHandler<SendConfirmToDbMain, Response<SendConfirmToDbMainResponse>>
{
    public async Task<Response<SendConfirmToDbMainResponse>> Handle(SendConfirmToDbMain request, CancellationToken cancellationToken)
    {

        try
        {
            // Busca confirmacion en la Bd auxiliar 
            var confirmation = await processOrderConfirmationQuerySqlDB
                .FirstOrDefaultAsync(x => x.CommStatus == 1, request.DbChoice, false);

            if (confirmation == null)
            {
                await logger.LogInfoAsync($"No se encontró confirmación en la Db: {request.DbChoice}",
                    "Metodo: SendConfirmToDbMainHandler");
                return new Response<SendConfirmToDbMainResponse>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new SendConfirmToDbMainResponse
                    {
                        Result = false,
                        Message = $"No se encontró confirmación en la Db: {request.DbChoice}"
                    }
                };
            }

            // Busca los movimientos de material en la Bd auxiliar
            var movements = await materialMovementQuerySqlDB
                .WhereAsync(x => x.ProcessOrderConfirmationId == confirmation.Id, request.DbChoice, false);

            if (movements.Count == 0)
            {
                await logger.LogInfoAsync($"No se encontró movimiento de material en la confirmación: {confirmation.Id}", 
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

            // Adiciona la confirmacion y los movimientos de material en la Bd principal
            await uow.BeginTransactionAsync("SapScadaMain");
            try
            {
                await processOrderConfirmationCommandSqlDB.AddToTransactionAsync(confirmation, "SapScadaMain");
                var clonedMovements = movements
                    .Select(m => Clone.WithoutId(m))
                    .ToList();
                await materialMovementCommandSqlDB.AddRangeToTransactionAsync(clonedMovements, "SapScadaMain");
                await uow.CommitAsync();
                uow.Dispose();
                await logger.LogInfoAsync($"Adicion exitosa de la confirmacion y movimientos de materiales en la Db: SapScadaMain",
                    "Metodo: SendConfirmToDbMainHandler");

            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                await logger.LogErrorAsync($"Error al guardar la confirmación en la Db SapScadaMain: {ex.Message}", 
                    "Metodo: SendConfirmToDbMainHandler");
                return new Response<SendConfirmToDbMainResponse>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new SendConfirmToDbMainResponse
                    {
                        Result = false,
                        Message = string.Empty
                    }
                };
            }

            confirmation.CommStatus = 2;
            await processOrderConfirmationCommandSqlDB.UpdateAsync(confirmation, request.DbChoice);
            await logger.LogInfoAsync($"Actualizacion exitosa de la confirmacion CommStatus = 2 en la Db: {request.DbChoice}",
                "Metodo: SendConfirmToDbMainHandler");

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
            await logger.LogErrorAsync($"Error: {ex.Message}",
                "Metodo: SendConfirmToDbMainHandler");
            return new Response<SendConfirmToDbMainResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new SendConfirmToDbMainResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }

}
