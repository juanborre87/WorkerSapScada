namespace Domain.Models
{
    public class OrderConfirmationRequest
    {
        public string OrderID { get; set; } = default!;
        public string ConfirmationUnit { get; set; } = default!;
        public string ConfirmationUnitISOCode { get; set; } = default!;
        public string ConfirmationYieldQuantity { get; set; } = default!;
        public List<OrderConfirmationMaterialItem> To_ProcOrdConfMatlDocItm { get; set; } = [];
    }

    public class OrderConfirmationMaterialItem
    {
        public string OrderID { get; set; } = default!;
        public string Material { get; set; } = default!;
        public string Plant { get; set; } = default!;
        public string StorageLocation { get; set; } = default!;
        public string GoodsMovementType { get; set; } = default!;
        public string EntryUnit { get; set; } = default!;
        public string QuantityInEntryUnit { get; set; } = default!;
    }
}
