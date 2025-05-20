namespace EBISX_POS.API.Services.DTO.Report
{
    public class GetInvoicesDTO
    {
        public required long InvoiceNum { get; set; }
        public required string InvoiceNumString { get; set; }
        public required string Date { get; set; }
        public required string Time { get; set; }
        public required string CashierEmail { get; set; }
        public required string CashierName { get; set; }
        public required string InvoiceStatus { get; set; }
    }
}
