using System.Globalization;
using System.Text.Json.Serialization;

namespace EBISX_POS.API.Services.DTO.Report
{
    public class ZInvoiceReportDTO
    {
        // Business Header
        public required string BusinessName { get; set; }
        public required string OperatorName { get; set; }
        public required string AddressLine { get; set; }
        public required string VatRegTin { get; set; }
        public required string Min { get; set; }
        public required string SerialNumber { get; set; }
        public required string TransactCount { get; set; }

        // Report Info
        public required string ReportDate { get; set; }
        public required string ReportTime { get; set; }
        public required string StartDateTime { get; set; }
        public required string EndDateTime { get; set; }

        // Invoice Numbers
        public required string BeginningSI { get; set; }
        public required string EndingSI { get; set; }
        public required string BeginningVoid { get; set; }
        public required string EndingVoid { get; set; }
        public required string BeginningReturn { get; set; }
        public required string EndingReturn { get; set; }

        // Counter Info
        public required string ResetCounter { get; set; }
        public required string ZCounter { get; set; }

        // Sales Summary
        public required string PresentAccumulatedSales { get; set; }
        public required string PreviousAccumulatedSales { get; set; }
        public required string SalesForTheDay { get; set; }

        public required SalesBreakdown SalesBreakdown { get; set; }
        public required DiscountSummary DiscountSummary { get; set; }
        public required SalesAdjustment SalesAdjustment { get; set; }
        public required VatAdjustment VatAdjustment { get; set; }

        public required TransactionSummary TransactionSummary { get; set; }

        public required string OpeningFund { get; set; }
        public required string Withdrawal { get; set; }
        public required string PaymentsReceived { get; set; }

        public required string ShortOver { get; set; }
    }

    // Sub-DTOs

    public class SalesBreakdown
    {
        public required string VatableSales { get; set; }
        public required string VatAmount { get; set; }
        public required string VatExemptSales { get; set; }
        public required string ZeroRatedSales { get; set; }

        public required string GrossAmount { get; set; }
        public required string LessDiscount { get; set; }
        public required string LessReturn { get; set; }
        public required string LessVoid { get; set; }
        public required string LessVatAdjustment { get; set; }
        public required string NetAmount { get; set; }
    }

    public class DiscountSummary
    {
        public required string SeniorCitizen { get; set; }
        public required string SeniorCitizenCount { get; set; }
        public required string PWD { get; set; }
        public required string PWDCount { get; set; }
        public required string Other { get; set; }
        public required string OtherCount { get; set; }
    }

    public class SalesAdjustment
    {
        public required string Void { get; set; }
        public required string VoidCount { get; set; }
        public required string Return { get; set; }
        public required string ReturnCount { get; set; }
    }

    public class VatAdjustment
    {
        public required string SCTrans { get; set; }
        public required string PWDTrans { get; set; }
        public required string RegDiscTrans { get; set; }
        public required string ZeroRatedTrans { get; set; }
        public required string VatOnReturn { get; set; }
        public required string OtherAdjustments { get; set; }
    }
}
