namespace EBISX_POS.API.Services.DTO.Report
{
    public class GetInvoiceDTO
    {
        // Business Details
        public string RegisteredName { get; set; } = string.Empty; // Default to empty string
        public string Address { get; set; } = string.Empty; // Default to empty string
        public string VatTinNumber { get; set; } = string.Empty; // Default to empty string
        public string MinNumber { get; set; } = string.Empty; // Default to empty string

        // Invoice Details
        public string InvoiceNum { get; set; } = string.Empty; // Default to empty string
        public string InvoiceDate { get; set; } = string.Empty; // Default to empty string
        public string OrderType { get; set; } = string.Empty; // Default to empty string
        public string CashierName { get; set; } = string.Empty; // Default to empty string

        // Items
        public List<ItemDTO> Items { get; set; } = new List<ItemDTO>(); // Default to empty list

        // Totals
        public string TotalAmount { get; set; } = string.Empty; // Default to empty string
        public string DiscountAmount { get; set; } = string.Empty; // Default to empty string
        public string DueAmount { get; set; } = string.Empty; // Default to empty string
        public List<OtherPaymentDTO>? OtherPayments { get; set; } = new List<OtherPaymentDTO>(); // Default to empty list
        public string CashTenderAmount { get; set; } = string.Empty; // Default to empty string
        public string TotalTenderAmount { get; set; } = string.Empty; // Default to empty string
        public string ChangeAmount { get; set; } = string.Empty; // Default to empty string
        public string VatExemptSales { get; set; } = string.Empty; // Default to empty string
        public string VatSales { get; set; } = string.Empty; // Default to empty string
        public string VatAmount { get; set; } = string.Empty; // Default to empty string

        public List<string> ElligiblePeopleDiscounts { get; set; } = new List<string>(); // Default to empty list

        // POS Details
        public string PosSerialNumber { get; set; } = string.Empty; // Default to empty string
        public string DateIssued { get; set; } = string.Empty; // Default to empty string
        public string ValidUntil { get; set; } = string.Empty; // Default to empty string
    }

    public class ItemDTO
    {
        public int Qty { get; set; } = 0; // Default to 0
        public List<ItemInfoDTO> itemInfos { get; set; } = new List<ItemInfoDTO>(); // Default to empty list
    }

    public class ItemInfoDTO
    {
        public string Description { get; set; } = string.Empty; // Default to empty string
        public string Amount { get; set; } = string.Empty; // Default to empty string
    }

    public class OtherPaymentDTO
    {
        public string SaleTypeName { get; set; } = string.Empty; // Default to empty string
        public string Amount { get; set; } = string.Empty; // Default to empty string
    }
}
