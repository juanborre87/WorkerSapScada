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
            //OrderOperation = null,
            //OrderSuboperation = null,
            //OrderType = confirmation.Order?.ManufacturingOrderType,
            //OrderOperationInternalID = null,
            ConfirmationText = confirmation.ConfirmationText,
            Language = "ES", // puedes ajustar si es dinámico
            //Material = confirmation.Order?.Material,
            //OrderPlannedTotalQty = confirmation.Order?.TotalQuantity?.ToString() ?? "0",
            //ProductionUnit = confirmation.Order?.ProductionUnit,
            FinalConfirmationType = confirmation.FinalConfirmationType,
            IsFinalConfirmation = confirmation.IsFinalConfirmation ?? false,
            OpenReservationsIsCleared = false,
            IsReversed = false,
            IsReversal = false,
            APIConfHasNoGoodsMovements = movements == null || movements.Count == 0,
            //OrderConfirmationRecordType = null,
            //ConfirmationEntryDate = FormatSapDate(confirmation.ConfirmationEntryDateTime),
            //ConfirmationEntryTime = FormatSapTime(confirmation.ConfirmationEntryDateTime),
            //EnteredByUser = confirmation.EnteredByUser,
            //LastChangeDate = FormatSapDate(confirmation.Order?.LastChangeDateTime),
            //LastChangedByUser = null,
            //ConfirmationExternalEntryDate = FormatSapDate(confirmation.InterfaceTimestamp),
            //ConfirmationExternalEntryTime = FormatSapTime(confirmation.InterfaceTimestamp),
            //EnteredByExternalUser = null,
            //ExternalSystemConfirmation = null,
            //Plant = confirmation.Plant,
            //WorkCenterTypeCode = null,
            //WorkCenter = confirmation.WorkCenter,
            //ShiftGrouping = null,
            //ShiftDefinition = null,
            Personnel = confirmation.Personnel,
            //TimeRecording = null,
            //EmployeeWageType = null,
            //EmployeeWageGroup = null,
            //BreakDurationUnit = null,
            //BreakDurationUnitISOCode = null,
            //BreakDurationUnitSAPCode = null,
            ConfirmedBreakDuration = "0",
            //EmployeeSuitability = null,
            NumberOfEmployees = "0",
            PostingDate = FormatSapDate(confirmation.PostingDate),
            ConfirmedExecutionStartDate = FormatSapDate(confirmation.Order?.MfgOrderScheduledStartDateTime),
            ConfirmedExecutionStartTime = FormatSapTime(confirmation.Order?.MfgOrderScheduledStartDateTime),
            //ConfirmedSetupEndDate = null,
            //ConfirmedSetupEndTime = null,
            //ConfirmedProcessingStartDate = null,
            //ConfirmedProcessingStartTime = null,
            //ConfirmedProcessingEndDate = null,
            //ConfirmedProcessingEndTime = null,
            //ConfirmedTeardownStartDate = null,
            //ConfirmedTeardownStartTime = null,
            ConfirmedExecutionEndDate = FormatSapDate(confirmation.Order?.MfgOrderScheduledEndDateTime),
            ConfirmedExecutionEndTime = FormatSapTime(confirmation.Order?.MfgOrderScheduledEndDateTime),
            ConfirmationUnit = confirmation.ConfirmationUnit,
            ConfirmationUnitISOCode = confirmation.ConfirmationUnitIsocode,
            ConfirmationUnitSAPCode = confirmation.ConfirmationUnitSapcode,
            ConfirmationYieldQuantity = confirmation.ConfirmationYieldQuantity?.ToString() ?? "0",
            ConfirmationScrapQuantity = confirmation.ConfirmationScrapQuantity?.ToString() ?? "0",
            VarianceReasonCode = confirmation.VarianceReasonCode,
            // puedes mapear más cantidades de trabajo si tienes datos
            //OpWorkQuantityUnit1 = null,
            //WorkQuantityUnit1ISOCode = null,
            //WorkQuantityUnit1SAPCode = null,
            //OpConfirmedWorkQuantity1 = "0",
            //NoFurtherOpWorkQuantity1IsExpd = true,
            //OpWorkQuantityUnit2 = null,
            //WorkQuantityUnit2ISOCode = null,
            //WorkQuantityUnit2SAPCode = null,
            //OpConfirmedWorkQuantity2 = "0",
            //NoFurtherOpWorkQuantity2IsExpd = true,
            //OpWorkQuantityUnit3 = null,
            //WorkQuantityUnit3ISOCode = null,
            //WorkQuantityUnit3SAPCode = null,
            //OpConfirmedWorkQuantity3 = "0",
            //NoFurtherOpWorkQuantity3IsExpd = true,
            //OpWorkQuantityUnit4 = null,
            //WorkQuantityUnit4ISOCode = null,
            //WorkQuantityUnit4SAPCode = null,
            //OpConfirmedWorkQuantity4 = "0",
            //NoFurtherOpWorkQuantity4IsExpd = true,
            //OpWorkQuantityUnit5 = null,
            //WorkQuantityUnit5ISOCode = null,
            //WorkQuantityUnit5SAPCode = null,
            //OpConfirmedWorkQuantity5 = "0",
            //NoFurtherOpWorkQuantity5IsExpd = true,
            //OpWorkQuantityUnit6 = null,
            //WorkQuantityUnit6ISOCode = null,
            //WorkQuantityUnit6SAPCode = null,
            //OpConfirmedWorkQuantity6 = "0",
            //NoFurtherOpWorkQuantity6IsExpd = true,
            //BusinessProcessEntryUnit = null,
            //BusProcessEntrUnitISOCode = null,
            //BusProcessEntryUnitSAPCode = null,
            //BusinessProcessConfirmedQty = "0",
            //NoFurtherBusinessProcQtyIsExpd = true,

            ToProcOrdConfMatlDocItm = new MaterialDocumentItemResults
            {
                Results = movements.Select(m =>
                {
                    var component = m.ProcessOrderComponent;
                    return new MaterialDocumentItem
                    {
                        //OrderType = confirmation.Order?.ManufacturingOrderType,
                        OrderID = confirmation.OrderId,
                        //OrderItem = null,
                        ManufacturingOrderCategory = confirmation.Order?.ManufacturingOrderCategory,
                        Material = component?.Material,
                        Plant = component?.ManufacturingOrderNavigation?.Plant ?? confirmation.Plant,
                        Reservation = component?.Reservation,
                        ReservationItem = component?.ReservationItem,
                        StorageLocation = component?.StorageLocation,
                        //ProductionSupplyArea = null,
                        Batch = component?.Batch,
                        //InventoryValuationType = null,
                        GoodsMovementType = component?.GoodsMovementType,
                        //GoodsMovementReasonCode = null,
                        //GoodsMovementRefDocType = null,
                        //InventoryUsabilityCode = null,
                        //InventorySpecialStockType = null,
                        //SalesOrder = null,
                        //SalesOrderItem = null,
                        //WBSElementExternalID = null,
                        //Supplier = null,
                        //Customer = null,
                        ReservationIsFinallyIssued = false,
                        IsCompletelyDelivered = false,
                        ShelfLifeExpirationDate = FormatSapDate(confirmation.Expiration),
                        ManufactureDate = FormatSapDate(confirmation.ConfirmationEntryDateTime),
                        //StorageType = null,
                        //StorageBin = null,
                        //EWMWarehouse = null,
                        //EWMStorageBin = null,
                        //MaterialDocumentItemText = null,
                        EntryUnit = component.EntryUnit,
                        EntryUnitISOCode = component.EntryUnitIsocode,
                        EntryUnitSAPCode = component.EntryUnitSapcode,
                        QuantityInEntryUnit = m.QuantityInEntryUnit?.ToString() ?? "0",
                        ToProcOrderConfBatchCharc = new BatchCharacteristicsResults
                        {
                            Results = [] // Agrega características si aplica
                        }
                    };
                }).ToList()
            }


        };
        //var materialItems = new List<MaterialDocumentItem>();

        //foreach (var m in movements)
        //{
        //    var component = m.ProcessOrderComponent;

        //    var item = new MaterialDocumentItem
        //    {
        //        OrderType = confirmation.Order?.ManufacturingOrderType,
        //        OrderID = confirmation.OrderId,
        //        OrderItem = null,
        //        ManufacturingOrderCategory = confirmation.Order?.ManufacturingOrderCategory,
        //        Material = component?.Material,
        //        Plant = component?.ManufacturingOrderNavigation?.Plant ?? confirmation.Plant,
        //        Reservation = component?.Reservation,
        //        ReservationItem = component?.ReservationItem,
        //        StorageLocation = component?.StorageLocation,
        //        ProductionSupplyArea = null,
        //        Batch = component?.Batch,
        //        InventoryValuationType = null,
        //        GoodsMovementType = component?.GoodsMovementType,
        //        GoodsMovementReasonCode = null,
        //        GoodsMovementRefDocType = null,
        //        InventoryUsabilityCode = null,
        //        InventorySpecialStockType = null,
        //        SalesOrder = null,
        //        SalesOrderItem = null,
        //        WBSElementExternalID = null,
        //        Supplier = null,
        //        Customer = null,
        //        ReservationIsFinallyIssued = false,
        //        IsCompletelyDelivered = false,
        //        ShelfLifeExpirationDate = FormatSapDate(confirmation.Expiration),
        //        ManufactureDate = FormatSapDate(confirmation.ConfirmationEntryDateTime),
        //        StorageType = null,
        //        StorageBin = null,
        //        EWMWarehouse = null,
        //        EWMStorageBin = null,
        //        MaterialDocumentItemText = null,
        //        EntryUnit = component.EntryUnit,
        //        EntryUnitISOCode = component.EntryUnitIsocode,
        //        EntryUnitSAPCode = component.EntryUnitSapcode,
        //        QuantityInEntryUnit = m.QuantityInEntryUnit?.ToString() ?? "0",
        //        ToProcOrderConfBatchCharc = new BatchCharacteristicsResults
        //        {
        //            Results = new List<BatchCharacteristic>() // agregar si aplica
        //        }
        //    };

        //    materialItems.Add(item);
        //}

        //var dtoMaterialResults = new MaterialDocumentItemResults
        //{
        //    Results = materialItems
        //};

        return dto;
        //var dto = new OrderConfirmationRequest
        //{
        //    OrderID = confirmation.OrderId,
        //    ConfirmationText = confirmation.ConfirmationText,
        //    FinalConfirmationType = confirmation.FinalConfirmationType,
        //    IsFinalConfirmation = confirmation.IsFinalConfirmation ?? false,
        //    //ConfirmationEntryDate = FormatSapDate(confirmation.ConfirmationEntryDateTime),
        //    //ConfirmationEntryTime = FormatSapTime(confirmation.ConfirmationEntryDateTime),
        //    //EnteredByUser = confirmation.EnteredByUser,
        //    Plant = confirmation.Plant,
        //    WorkCenter = confirmation.WorkCenter,
        //    Personnel = confirmation.Personnel,
        //    PostingDate = FormatSapDate(confirmation.PostingDate),
        //    ConfirmationUnit = confirmation.ConfirmationUnit,
        //    ConfirmationUnitISOCode = confirmation.ConfirmationUnitIsocode,
        //    ConfirmationUnitSAPCode = confirmation.ConfirmationUnitSapcode,
        //    ConfirmationYieldQuantity = confirmation.ConfirmationYieldQuantity?.ToString() ?? "0",
        //    ConfirmationScrapQuantity = confirmation.ConfirmationScrapQuantity?.ToString() ?? "0",
        //    VarianceReasonCode = confirmation.VarianceReasonCode,
        //    APIConfHasNoGoodsMovements = movements == null || movements.Count == 0,

        //    ToProcOrdConfMatlDocItm = new MaterialDocumentItemResults
        //    {
        //        Results = movements.Select(m => new MaterialDocumentItem
        //        {
        //            OrderID = confirmation.OrderId,
        //            Material = confirmation.Order?.Material,
        //            Plant = confirmation.Plant,
        //            Batch = confirmation.Batch,
        //            EntryUnit = m.EntryUnit,
        //            EntryUnitISOCode = m.EntryUnitIsocode,
        //            EntryUnitSAPCode = m.EntryUnitSapcode,
        //            QuantityInEntryUnit = m.QuantityInEntryUnit?.ToString() ?? "0",
        //            ShelfLifeExpirationDate = FormatSapDate(confirmation.Expiration),
        //            ManufactureDate = FormatSapDate(confirmation.ConfirmationEntryDateTime),
        //            // Puedes completar los demás campos según tu necesidad
        //            ToProcOrderConfBatchCharc = new BatchCharacteristicsResults
        //            {
        //                Results = [] // si tienes características de lote
        //            }
        //        }).ToList()
        //    }
        //};

        //return dto;
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
