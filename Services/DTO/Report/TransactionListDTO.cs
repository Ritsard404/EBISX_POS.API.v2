namespace EBISX_POS.API.Services.DTO.Report
{
    public class TransactionListDTO
    {
        public string Date { get; set; }
        public string InvoiceNum { get; set; }
        public string Src { get; set; }
        public string DiscType { get; set; }
        public string Percent { get; set; }
        public decimal SubTotal { get; set; }
        public decimal AmountDue { get; set; }
        public decimal GrossSales { get; set; }
        public decimal Returns { get; set; }
        public decimal NetOfReturns { get; set; }
        public decimal LessDiscount { get; set; }
        public decimal NetOfSales { get; set; }
        public decimal Vatable { get; set; }
        public decimal ZeroRated { get; set; }
        public decimal Exempt { get; set; }
        public decimal Vat { get; set; }
    }
    public class TotalTransactionListDTO
    {
        public decimal TotalGrossSales { get; set; }
        public decimal TotalReturns { get; set; }
        public decimal TotalNetOfReturns { get; set; }
        public decimal TotalLessDiscount { get; set; }
        public decimal TotalNetOfSales { get; set; }
        public decimal TotalVatable { get; set; }
        public decimal TotalExempt { get; set; }
        public decimal TotalVat { get; set; }
    }


} 