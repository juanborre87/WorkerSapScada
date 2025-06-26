using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using HostWorker.Models;
using MediatR;
using System.Net;

namespace Application.UseCases.Operation.Commands.Update;

public class UpdateCommand : IRequest<Response<UpdateResponse>>
{
}

public class UpdateCommandHandler(
    IQuerySqlDB<ProcessOrderConfirmation> processOrderConfirmationQuerySqlDB,
    IQuerySqlDB<ProcessOrder> processOrderQuerySqlDB,
    IQuerySqlDB<ProcessOrderComponent> processOrderComponentQuerySqlDB,
    ICommandSqlDB<ProcessOrderConfirmation> processOrderCommandSqlDB,
    ISapOrderService sapOrderService) :
    IRequestHandler<UpdateCommand, Response<UpdateResponse>>
{
    public async Task<Response<UpdateResponse>> Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var confirmation = await processOrderConfirmationQuerySqlDB.FirstOrDefaultAsync(x => x.Status == 0);

            //if (confirmation == null)
            //{
            //    return new Response<UpdateResponse>
            //    {
            //        StatusCode = HttpStatusCode.NotFound,
            //        Content = new UpdateResponse { Result = false, Message = "No confirmation found." }
            //    };
            //}

            var processOrder = await processOrderQuerySqlDB.FirstOrDefaultAsync(x => x.ManufacturingOrder == confirmation.OrderId);
            var processOrderComponent = await processOrderComponentQuerySqlDB.FirstOrDefaultAsync(x => x.ManufacturingOrder == confirmation.OrderId);

            // Mapeo manual a la estructura esperada por SAP
            //var sapRequest = new OrderConfirmationRequest
            //{
            //    OrderID = confirmation.OrderId.ToString(),
            //    ConfirmationUnit = confirmation.ConfirmationUnit,
            //    ConfirmationUnitISOCode = confirmation.ConfirmationUnitIsocode,
            //    ConfirmationYieldQuantity = confirmation.ConfirmationYieldQuantity.ToString(),
            //    To_ProcOrdConfMatlDocItm =
            //    [
            //        new OrderConfirmationMaterialItem
            //        {
            //            OrderID = confirmation.OrderId.ToString(),
            //            Material = processOrder.Material,
            //            Plant = confirmation.Plant,
            //            StorageLocation = processOrder.StorageLocation,
            //            GoodsMovementType = processOrderComponent.GoodsMovementType,
            //            EntryUnit = processOrderComponent.EntryUnit,
            //            QuantityInEntryUnit = null/////////////////////////////////////////
            //        }
            //    ]
            //};

            var sapRequest = new OrderConfirmationRequest
            {
                OrderID = "510000000002",
                ConfirmationUnit = "PC",
                ConfirmationUnitISOCode = "PCE",
                ConfirmationYieldQuantity = "1",
                To_ProcOrdConfMatlDocItm =
                [
                    new OrderConfirmationMaterialItem
                    {
                        OrderID = "510000000002",
                        Material = "16-008410",
                        Plant = "5000",
                        StorageLocation = "5020",
                        GoodsMovementType = "261",
                        EntryUnit = "PC",
                        QuantityInEntryUnit = "1.000"
                    }
                ]
            };

            var result = await sapOrderService.SendOrderConfirmationAsync(sapRequest);

            return new Response<UpdateResponse>
            {
                StatusCode = result ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                Content = new UpdateResponse
                {
                    Result = result,
                    Message = result ? "Confirmation sent to SAP." : "Failed to send confirmation to SAP."
                }
            };
        }
        catch (Exception ex)
        {
            return new Response<UpdateResponse>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new UpdateResponse
                {
                    Result = false,
                    Message = $"Error: {ex.Message}"
                }
            };
        }

    }
}
