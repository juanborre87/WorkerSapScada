using Newtonsoft.Json;

namespace Domain.Models
{
    public class OrderConfirmationRequest
    {
        [JsonProperty("OrderID")]
        public string OrderID { get; set; }

        [JsonProperty("OrderOperation")]
        public string OrderOperation { get; set; }

        [JsonProperty("OrderSuboperation")]
        public string OrderSuboperation { get; set; }

        [JsonProperty("OrderType")]
        public string OrderType { get; set; }

        [JsonProperty("OrderOperationInternalID")]
        public string OrderOperationInternalID { get; set; }

        [JsonProperty("ConfirmationText")]
        public string ConfirmationText { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Material")]
        public string Material { get; set; }

        [JsonProperty("OrderPlannedTotalQty")]
        public string OrderPlannedTotalQty { get; set; }

        [JsonProperty("ProductionUnit")]
        public string ProductionUnit { get; set; }

        [JsonProperty("FinalConfirmationType")]
        public string FinalConfirmationType { get; set; }

        [JsonProperty("IsFinalConfirmation")]
        public bool IsFinalConfirmation { get; set; }

        [JsonProperty("OpenReservationsIsCleared")]
        public bool OpenReservationsIsCleared { get; set; }

        [JsonProperty("IsReversed")]
        public bool IsReversed { get; set; }

        [JsonProperty("IsReversal")]
        public bool IsReversal { get; set; }

        [JsonProperty("APIConfHasNoGoodsMovements")]
        public bool APIConfHasNoGoodsMovements { get; set; }

        [JsonProperty("OrderConfirmationRecordType")]
        public string OrderConfirmationRecordType { get; set; }

        //[JsonProperty("ConfirmationEntryDate")]
        //public string ConfirmationEntryDate { get; set; }

        //[JsonProperty("ConfirmationEntryTime")]
        //public string ConfirmationEntryTime { get; set; }

        //[JsonProperty("EnteredByUser")]
        //public string EnteredByUser { get; set; }

        [JsonProperty("LastChangeDate")]
        public string LastChangeDate { get; set; }

        [JsonProperty("LastChangedByUser")]
        public string LastChangedByUser { get; set; }

        //[JsonProperty("ConfirmationExternalEntryDate")]
        //public string ConfirmationExternalEntryDate { get; set; }

        //[JsonProperty("ConfirmationExternalEntryTime")]
        //public string ConfirmationExternalEntryTime { get; set; }

        //[JsonProperty("EnteredByExternalUser")]
        //public string EnteredByExternalUser { get; set; }

        [JsonProperty("ExternalSystemConfirmation")]
        public string ExternalSystemConfirmation { get; set; }

        [JsonProperty("Plant")]
        public string Plant { get; set; }

        [JsonProperty("WorkCenterTypeCode")]
        public string WorkCenterTypeCode { get; set; }

        [JsonProperty("WorkCenter")]
        public string WorkCenter { get; set; }

        [JsonProperty("ShiftGrouping")]
        public string ShiftGrouping { get; set; }

        [JsonProperty("ShiftDefinition")]
        public string ShiftDefinition { get; set; }

        [JsonProperty("Personnel")]
        public string Personnel { get; set; }

        [JsonProperty("TimeRecording")]
        public string TimeRecording { get; set; }

        [JsonProperty("EmployeeWageType")]
        public string EmployeeWageType { get; set; }

        [JsonProperty("EmployeeWageGroup")]
        public string EmployeeWageGroup { get; set; }

        [JsonProperty("BreakDurationUnit")]
        public string BreakDurationUnit { get; set; }

        [JsonProperty("BreakDurationUnitISOCode")]
        public string BreakDurationUnitISOCode { get; set; }

        [JsonProperty("BreakDurationUnitSAPCode")]
        public string BreakDurationUnitSAPCode { get; set; }

        [JsonProperty("ConfirmedBreakDuration")]
        public string ConfirmedBreakDuration { get; set; }

        [JsonProperty("EmployeeSuitability")]
        public string EmployeeSuitability { get; set; }

        [JsonProperty("NumberOfEmployees")]
        public string NumberOfEmployees { get; set; }

        [JsonProperty("PostingDate")]
        public string PostingDate { get; set; }

        [JsonProperty("ConfirmedExecutionStartDate")]
        public string ConfirmedExecutionStartDate { get; set; }

        [JsonProperty("ConfirmedExecutionStartTime")]
        public string ConfirmedExecutionStartTime { get; set; }

        [JsonProperty("ConfirmedSetupEndDate")]
        public string ConfirmedSetupEndDate { get; set; }

        [JsonProperty("ConfirmedSetupEndTime")]
        public string ConfirmedSetupEndTime { get; set; }

        [JsonProperty("ConfirmedProcessingStartDate")]
        public string ConfirmedProcessingStartDate { get; set; }

        [JsonProperty("ConfirmedProcessingStartTime")]
        public string ConfirmedProcessingStartTime { get; set; }

        [JsonProperty("ConfirmedProcessingEndDate")]
        public string ConfirmedProcessingEndDate { get; set; }

        [JsonProperty("ConfirmedProcessingEndTime")]
        public string ConfirmedProcessingEndTime { get; set; }

        [JsonProperty("ConfirmedTeardownStartDate")]
        public string ConfirmedTeardownStartDate { get; set; }

        [JsonProperty("ConfirmedTeardownStartTime")]
        public string ConfirmedTeardownStartTime { get; set; }

        [JsonProperty("ConfirmedExecutionEndDate")]
        public string ConfirmedExecutionEndDate { get; set; }

        [JsonProperty("ConfirmedExecutionEndTime")]
        public string ConfirmedExecutionEndTime { get; set; }

        [JsonProperty("ConfirmationUnit")]
        public string ConfirmationUnit { get; set; }

        [JsonProperty("ConfirmationUnitISOCode")]
        public string ConfirmationUnitISOCode { get; set; }

        [JsonProperty("ConfirmationUnitSAPCode")]
        public string ConfirmationUnitSAPCode { get; set; }

        [JsonProperty("ConfirmationYieldQuantity")]
        public string ConfirmationYieldQuantity { get; set; }

        [JsonProperty("ConfirmationScrapQuantity")]
        public string ConfirmationScrapQuantity { get; set; }

        [JsonProperty("VarianceReasonCode")]
        public string VarianceReasonCode { get; set; }

        // Unidades de trabajo confirmadas (hasta 6)
        [JsonProperty("OpWorkQuantityUnit1")]
        public string OpWorkQuantityUnit1 { get; set; }
        [JsonProperty("WorkQuantityUnit1ISOCode")]
        public string WorkQuantityUnit1ISOCode { get; set; }
        [JsonProperty("WorkQuantityUnit1SAPCode")]
        public string WorkQuantityUnit1SAPCode { get; set; }
        [JsonProperty("OpConfirmedWorkQuantity1")]
        public string OpConfirmedWorkQuantity1 { get; set; }
        [JsonProperty("NoFurtherOpWorkQuantity1IsExpd")]
        public bool NoFurtherOpWorkQuantity1IsExpd { get; set; }

        [JsonProperty("OpWorkQuantityUnit2")]
        public string OpWorkQuantityUnit2 { get; set; }
        [JsonProperty("WorkQuantityUnit2ISOCode")]
        public string WorkQuantityUnit2ISOCode { get; set; }
        [JsonProperty("WorkQuantityUnit2SAPCode")]
        public string WorkQuantityUnit2SAPCode { get; set; }
        [JsonProperty("OpConfirmedWorkQuantity2")]
        public string OpConfirmedWorkQuantity2 { get; set; }
        [JsonProperty("NoFurtherOpWorkQuantity2IsExpd")]
        public bool NoFurtherOpWorkQuantity2IsExpd { get; set; }

        [JsonProperty("OpWorkQuantityUnit3")]
        public string OpWorkQuantityUnit3 { get; set; }
        [JsonProperty("WorkQuantityUnit3ISOCode")]
        public string WorkQuantityUnit3ISOCode { get; set; }
        [JsonProperty("WorkQuantityUnit3SAPCode")]
        public string WorkQuantityUnit3SAPCode { get; set; }
        [JsonProperty("OpConfirmedWorkQuantity3")]
        public string OpConfirmedWorkQuantity3 { get; set; }
        [JsonProperty("NoFurtherOpWorkQuantity3IsExpd")]
        public bool NoFurtherOpWorkQuantity3IsExpd { get; set; }

        [JsonProperty("OpWorkQuantityUnit4")]
        public string OpWorkQuantityUnit4 { get; set; }
        [JsonProperty("WorkQuantityUnit4ISOCode")]
        public string WorkQuantityUnit4ISOCode { get; set; }
        [JsonProperty("WorkQuantityUnit4SAPCode")]
        public string WorkQuantityUnit4SAPCode { get; set; }
        [JsonProperty("OpConfirmedWorkQuantity4")]
        public string OpConfirmedWorkQuantity4 { get; set; }
        [JsonProperty("NoFurtherOpWorkQuantity4IsExpd")]
        public bool NoFurtherOpWorkQuantity4IsExpd { get; set; }

        [JsonProperty("OpWorkQuantityUnit5")]
        public string OpWorkQuantityUnit5 { get; set; }
        [JsonProperty("WorkQuantityUnit5ISOCode")]
        public string WorkQuantityUnit5ISOCode { get; set; }
        [JsonProperty("WorkQuantityUnit5SAPCode")]
        public string WorkQuantityUnit5SAPCode { get; set; }
        [JsonProperty("OpConfirmedWorkQuantity5")]
        public string OpConfirmedWorkQuantity5 { get; set; }
        [JsonProperty("NoFurtherOpWorkQuantity5IsExpd")]
        public bool NoFurtherOpWorkQuantity5IsExpd { get; set; }

        [JsonProperty("OpWorkQuantityUnit6")]
        public string OpWorkQuantityUnit6 { get; set; }
        [JsonProperty("WorkQuantityUnit6ISOCode")]
        public string WorkQuantityUnit6ISOCode { get; set; }
        [JsonProperty("WorkQuantityUnit6SAPCode")]
        public string WorkQuantityUnit6SAPCode { get; set; }
        [JsonProperty("OpConfirmedWorkQuantity6")]
        public string OpConfirmedWorkQuantity6 { get; set; }
        [JsonProperty("NoFurtherOpWorkQuantity6IsExpd")]
        public bool NoFurtherOpWorkQuantity6IsExpd { get; set; }

        [JsonProperty("BusinessProcessEntryUnit")]
        public string BusinessProcessEntryUnit { get; set; }

        [JsonProperty("BusProcessEntrUnitISOCode")]
        public string BusProcessEntrUnitISOCode { get; set; }

        [JsonProperty("BusProcessEntryUnitSAPCode")]
        public string BusProcessEntryUnitSAPCode { get; set; }

        [JsonProperty("BusinessProcessConfirmedQty")]
        public string BusinessProcessConfirmedQty { get; set; }

        [JsonProperty("NoFurtherBusinessProcQtyIsExpd")]
        public bool NoFurtherBusinessProcQtyIsExpd { get; set; }

        [JsonProperty("to_ProcOrdConfMatlDocItm")]
        public MaterialDocumentItemResults ToProcOrdConfMatlDocItm { get; set; }
    }

    public class MaterialDocumentItemResults
    {
        [JsonProperty("results")]
        public List<MaterialDocumentItem> Results { get; set; }
    }

    public class MaterialDocumentItem
    {
        [JsonProperty("OrderType")]
        public string OrderType { get; set; }

        [JsonProperty("OrderID")]
        public string OrderID { get; set; }

        [JsonProperty("OrderItem")]
        public string OrderItem { get; set; }

        [JsonProperty("ManufacturingOrderCategory")]
        public string ManufacturingOrderCategory { get; set; }

        [JsonProperty("Material")]
        public string Material { get; set; }

        [JsonProperty("Plant")]
        public string Plant { get; set; }

        [JsonProperty("Reservation")]
        public string Reservation { get; set; }

        [JsonProperty("ReservationItem")]
        public string ReservationItem { get; set; }

        [JsonProperty("StorageLocation")]
        public string StorageLocation { get; set; }

        [JsonProperty("ProductionSupplyArea")]
        public string ProductionSupplyArea { get; set; }

        [JsonProperty("Batch")]
        public string Batch { get; set; }

        [JsonProperty("InventoryValuationType")]
        public string InventoryValuationType { get; set; }

        [JsonProperty("GoodsMovementType")]
        public string GoodsMovementType { get; set; }

        [JsonProperty("GoodsMovementReasonCode")]
        public string GoodsMovementReasonCode { get; set; }

        [JsonProperty("GoodsMovementRefDocType")]
        public string GoodsMovementRefDocType { get; set; }

        [JsonProperty("InventoryUsabilityCode")]
        public string InventoryUsabilityCode { get; set; }

        [JsonProperty("InventorySpecialStockType")]
        public string InventorySpecialStockType { get; set; }

        [JsonProperty("SalesOrder")]
        public string SalesOrder { get; set; }

        [JsonProperty("SalesOrderItem")]
        public string SalesOrderItem { get; set; }

        [JsonProperty("WBSElementExternalID")]
        public string WBSElementExternalID { get; set; }

        [JsonProperty("Supplier")]
        public string Supplier { get; set; }

        [JsonProperty("Customer")]
        public string Customer { get; set; }

        [JsonProperty("ReservationIsFinallyIssued")]
        public bool ReservationIsFinallyIssued { get; set; }

        [JsonProperty("IsCompletelyDelivered")]
        public bool IsCompletelyDelivered { get; set; }

        [JsonProperty("ShelfLifeExpirationDate")]
        public string ShelfLifeExpirationDate { get; set; }

        [JsonProperty("ManufactureDate")]
        public string ManufactureDate { get; set; }

        [JsonProperty("StorageType")]
        public string StorageType { get; set; }

        [JsonProperty("StorageBin")]
        public string StorageBin { get; set; }

        [JsonProperty("EWMWarehouse")]
        public string EWMWarehouse { get; set; }

        [JsonProperty("EWMStorageBin")]
        public string EWMStorageBin { get; set; }

        [JsonProperty("MaterialDocumentItemText")]
        public string MaterialDocumentItemText { get; set; }

        [JsonProperty("EntryUnit")]
        public string EntryUnit { get; set; }

        [JsonProperty("EntryUnitISOCode")]
        public string EntryUnitISOCode { get; set; }

        [JsonProperty("EntryUnitSAPCode")]
        public string EntryUnitSAPCode { get; set; }

        [JsonProperty("QuantityInEntryUnit")]
        public string QuantityInEntryUnit { get; set; }

        [JsonProperty("to_ProcOrderConfBatchCharc")]
        public BatchCharacteristicsResults ToProcOrderConfBatchCharc { get; set; }
    }

    public class BatchCharacteristicsResults
    {
        [JsonProperty("results")]
        public List<BatchCharacteristic> Results { get; set; }
    }

    public class BatchCharacteristic
    {
        [JsonProperty("Characteristic")]
        public string Characteristic { get; set; }

        [JsonProperty("CharcValue")]
        public string CharcValue { get; set; }
    }
}
