using System.Text.Json.Serialization;

namespace EBISX_POS.API.Services.DTO.Order
{
    public class FinalizeOrderDTO
    {
        public required string OrderType { get; set; }
        public required decimal TotalAmount { get; set; }
        public required decimal CashTendered { get; set; }
        public required decimal DiscountAmount { get; set; }

        public required decimal DueAmount { get; set; }
        public required decimal TotalTendered { get; set; }
        public required decimal ChangeAmount { get; set; }
        public required decimal VatExempt { get; set; }
        public required decimal VatSales { get; set; }
        public required decimal VatAmount { get; set; }

        public required string CashierEmail { get; set; }
    }
}
