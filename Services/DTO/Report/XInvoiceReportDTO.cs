
using System.Globalization;
using System.Text.Json.Serialization;

namespace EBISX_POS.API.Services.DTO.Report
{
    public class XInvoiceReportDTO
    {
        public required string BusinessName { get; set; }
        public required string OperatorName { get; set; }
        public required string AddressLine { get; set; }
        public required string VatRegTin { get; set; }
        public required string Min { get; set; }
        public required string SerialNumber { get; set; }

        public required string ReportDate { get; set; }
        public required string ReportTime { get; set; }
        public required string StartDateTime { get; set; }
        public required string EndDateTime { get; set; }

        public required string Cashier { get; set; }
        public required string BeginningOrNumber { get; set; }
        public required string EndingOrNumber { get; set; }

        public required string OpeningFund { get; set; }

        public required Payments Payments { get; set; }
        public required string VoidAmount { get; set; }
        public required string Refund { get; set; }
        public required string Withdrawal { get; set; }

        public required TransactionSummary TransactionSummary { get; set; }

        /// <summary>
        /// Use something like "1.60+" or "-0.40" here.
        /// </summary>
        public required string ShortOver { get; set; }
    }

    public class PaymentDetail
    {
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public decimal Amount { get; set; }
        public string AmountString => Amount.ToString("C", new CultureInfo("en-PH"));
    }

    public class Payments
    {
        [JsonIgnore]
        public decimal Cash { get; set; }
        public string CashString => Cash.ToString("C", new CultureInfo("en-PH"));

        // Renamed and typed for clarity
        public List<PaymentDetail> OtherPayments { get; set; } = new List<PaymentDetail>();

        public string Total => (Cash + OtherPayments.Sum(p => p.Amount)).ToString("C", new CultureInfo("en-PH"));
    }

    public class TransactionSummary
    {
        public required string CashInDrawer { get; set; }

        // Match payments in summary (e.g., cheque, credit card, etc.)
        public List<PaymentDetail> OtherPayments { get; set; } = new List<PaymentDetail>();
    }
}
