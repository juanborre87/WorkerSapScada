using Domain.Entities;
using Domain.Models;

namespace Application.Helpers;

public static class SapConfirmationMapper
{
    public static OrderConfirmationRequest MapToDto(ProcessOrderConfirmation confirmation, List<ProcessOrderConfirmationMaterialMovement> movements)
    {
        var dto = new OrderConfirmationRequest
        {
            OrderID = confirmation.OrderId,
            ConfirmationText = confirmation.ConfirmationText,
            FinalConfirmationType = confirmation.FinalConfirmationType,
            IsFinalConfirmation = confirmation.IsFinalConfirmation ?? false,
            ConfirmationEntryDate = FormatSapDate(confirmation.ConfirmationEntryDateTime),
            ConfirmationEntryTime = FormatSapTime(confirmation.ConfirmationEntryDateTime),
            EnteredByUser = confirmation.EnteredByUser,
            Plant = confirmation.Plant,
            WorkCenter = confirmation.WorkCenter,
            Personnel = confirmation.Personnel,
            PostingDate = FormatSapDate(confirmation.PostingDate),
            ConfirmationUnit = confirmation.ConfirmationUnit,
            ConfirmationUnitISOCode = confirmation.ConfirmationUnitIsocode,
            ConfirmationUnitSAPCode = confirmation.ConfirmationUnitSapcode,
            ConfirmationYieldQuantity = confirmation.ConfirmationYieldQuantity?.ToString() ?? "0",
            ConfirmationScrapQuantity = confirmation.ConfirmationScrapQuantity?.ToString() ?? "0",
            VarianceReasonCode = confirmation.VarianceReasonCode,
            APIConfHasNoGoodsMovements = movements == null || movements.Count == 0,

            ToProcOrdConfMatlDocItm = new MaterialDocumentItemResults
            {
                Results = movements.Select(m => new MaterialDocumentItem
                {
                    OrderID = confirmation.OrderId,
                    Material = confirmation.Order?.Material,
                    Plant = confirmation.Plant,
                    Batch = confirmation.Batch,
                    EntryUnit = m.EntryUnit,
                    EntryUnitISOCode = m.EntryUnitIsocode,
                    EntryUnitSAPCode = m.EntryUnitSapcode,
                    QuantityInEntryUnit = m.QuantityInEntryUnit?.ToString() ?? "0",
                    ShelfLifeExpirationDate = FormatSapDate(confirmation.Expiration),
                    ManufactureDate = FormatSapDate(confirmation.ConfirmationEntryDateTime),
                    // Puedes completar los demás campos según tu necesidad
                    ToProcOrderConfBatchCharc = new BatchCharacteristicsResults
                    {
                        Results = [] // si tienes características de lote
                    }
                }).ToList()
            }
        };

        return dto;
    }

    private static string FormatSapDate(DateTime? date)
    {
        return date.HasValue
            ? $"/Date({new DateTimeOffset(date.Value).ToUnixTimeMilliseconds()})/"
            : null;
    }

    private static string FormatSapTime(DateTime? date)
    {
        return date.HasValue
            ? $"PT{date.Value:HH}H{date.Value:mm}M{date.Value:ss}S"
            : null;
    }
}
