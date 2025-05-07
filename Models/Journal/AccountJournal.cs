using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBISX_POS.API.Models.Journal
{
    [Table("accountjournal")]
    public class AccountJournal
    {
        [Key]
        [Column("unique_id")]
        public long UniqueId { get; set; }  // NOT NULL, AUTO_INCREMENT

        [Required]
        [Column("Entry_Type")]
        public string EntryType { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("Entry_No")]
        public long? EntryNo { get; set; }  // DEFAULT NULL

        [Column("Entry_Line_No")]
        public int? EntryLineNo { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("Entry_Date")]
        public DateTime EntryDate { get; set; } = new DateTime(2001, 1, 1);  // NOT NULL, DEFAULT '2001-01-01'

        [Required]
        [Column("Entry_Name")]
        public string EntryName { get; set; } = "";  // NOT NULL

        [Required]
        [Column("group_id")]
        public string GroupId { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("AccountName")]
        public string AccountName { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("Description")]
        public string Description { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("Reference")]
        public string Reference { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("Branch")]
        public string Branch { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("TerminalNo")]
        public int? TerminalNo { get; set; } = 0;  // DEFAULT '0'

        [Column("Debit")]
        public double? Debit { get; set; } = 0;  // DEFAULT '0'

        [Column("Credit")]
        public double? Credit { get; set; } = 0;  // DEFAULT '0'

        [Column("AccountBalance")]
        public double? AccountBalance { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("Status")]
        public string Status { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("cleared")]
        public string Cleared { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("clearingref")]
        public int? ClearingRef { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("costcenter")]
        public string CostCenter { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("accountno")]
        public string AccountNo { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("costcenterdesc")]
        public string CostCenterDesc { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("linetype")]
        public string LineType { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("linetype_transno")]
        public int? LineTypeTransNo { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("ItemID")]
        public string ItemID { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("ItemDesc")]
        public string? ItemDesc { get; set; }  // TEXT, nullable

        [Required]
        [Column("Unit")]
        public string Unit { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("QtyIn")]
        public double? QtyIn { get; set; } = 0;  // DEFAULT '0'

        [Column("QtyOut")]
        public double? QtyOut { get; set; } = 0;  // DEFAULT '0'

        [Column("QtyPerBaseUnit")]
        public double? QtyPerBaseUnit { get; set; } = 1;  // DEFAULT '1'

        [Column("QtyBalanceInBaseUnit")]
        public double? QtyBalanceInBaseUnit { get; set; } = 0;  // DEFAULT '0'

        [Column("Cost")]
        public double? Cost { get; set; } = 0;  // DEFAULT '0'

        [Column("Price")]
        public double? Price { get; set; } = 0;  // DEFAULT '0'

        [Column("Discrate")]
        public double? DiscRate { get; set; } = 0;  // DEFAULT '0'

        [Column("Discamt")]
        public double? DiscAmt { get; set; } = 0;  // DEFAULT '0'

        [Column("TotalCost")]
        public double? TotalCost { get; set; } = 0;  // DEFAULT '0'

        [Column("TotalPrice")]
        public double? TotalPrice { get; set; } = 0;  // DEFAULT '0'

        [Column("received")]
        public double? Received { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("delivered", TypeName = "decimal(10,0)")]
        public decimal Delivered { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("tax_id")]
        public string TaxId { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("tax_account")]
        public string TaxAccount { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("tax_type")]
        public string TaxType { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("tax_rate")]
        public double? TaxRate { get; set; } = 0;  // DEFAULT '0'

        [Column("tax_total")]
        public double? TaxTotal { get; set; } = 0;  // DEFAULT '0'

        [Column("sub_total")]
        public double? SubTotal { get; set; } = 0;  // DEFAULT '0'

        [Column("serial")]
        public string? Serial { get; set; }  // TEXT, nullable

        [Required]
        [Column("chassis")]
        public string Chassis { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("engine")]
        public string Engine { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("itemtype")]
        public string ItemType { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("serialstatus")]
        public int? SerialStatus { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("expirydate")]
        public DateTime ExpiryDate { get; set; } = new DateTime(2001, 1, 1);  // NOT NULL, DEFAULT '2001-01-01'

        [Required]
        [Column("batchno")]
        public string BatchNo { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("itemcolor")]
        public string ItemColor { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("converted")]
        public int? Converted { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("vattype")]
        public string VatType { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("vatable")]
        public double? Vatable { get; set; } = 0;  // DEFAULT '0'

        [Column("exempt")]
        public double? Exempt { get; set; } = 0;  // DEFAULT '0'

        [Column("nonvatable")]
        public double? NonVatable { get; set; } = 0;  // DEFAULT '0'

        [Column("zerorated")]
        public double? ZeroRated { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("income_account")]
        public string IncomeAccount { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("cogs_account")]
        public string CogsAccount { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("inventory_account")]
        public string InventoryAccount { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("job_no")]
        public string? JobNo { get; set; }  // DEFAULT NULL

        [Required]
        [Column("job_desc")]
        public string JobDesc { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("name_type")]
        public string NameType { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("docref")]
        public string DocRef { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("name_desc")]
        public string NameDesc { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("length", TypeName = "decimal(12,2)")]
        public decimal? Length { get; set; } = 0.00m;  // DEFAULT '0.00'

        [Column("width", TypeName = "decimal(12,2)")]
        public decimal? Width { get; set; } = 0.00m;  // DEFAULT '0.00'

        [Column("area", TypeName = "decimal(12,2)")]
        public decimal? Area { get; set; } = 0.00m;  // DEFAULT '0.00'

        [Column("perimeter", TypeName = "decimal(12,2)")]
        public decimal? Perimeter { get; set; } = 0.00m;  // DEFAULT '0.00'

        [Required]
        [Column("fgid")]
        public string Fgid { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("illumination")]
        public string Illumination { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("size")]
        public string Size { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("face")]
        public string Face { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; } = new DateTime(2001, 1, 1);  // NOT NULL, DEFAULT '2001-01-01'

        [Required]
        [Column("location")]
        public string Location { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("principal")]
        public double? Principal { get; set; } = 0;  // DEFAULT '0'

        [Column("interest")]
        public double? Interest { get; set; } = 0;  // DEFAULT '0'

        [Column("penalty")]
        public double? Penalty { get; set; } = 0;  // DEFAULT '0'

        [Column("total_loan_amount")]
        public double? TotalLoanAmount { get; set; } = 0;  // DEFAULT '0'

        [Column("penalty_rate")]
        public double? PenaltyRate { get; set; } = 0;  // DEFAULT '0'

        [Column("penalty_term")]
        public int? PenaltyTerm { get; set; } = 0;  // DEFAULT '0'

        [Required]
        [Column("penalty_period")]
        public string PenaltyPeriod { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("bank")]
        public string Bank { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("check_number")]
        public string CheckNumber { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("amountinwords")]
        public string AmountInWords { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("voucher_date")]
        public DateTime VoucherDate { get; set; } = new DateTime(2001, 1, 1);  // NOT NULL, DEFAULT '2001-01-01'

        [Required]
        [Column("ship_to")]
        public string ShipTo { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("ship_to_name")]
        public string ShipToName { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("clerk")]
        public string Clerk { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Column("requested", TypeName = "decimal(12,2)")]
        public decimal? Requested { get; set; } = 0.00m;  // DEFAULT '0.00'

        [Column("entry_time")]
        public TimeSpan? EntryTime { get; set; } = new TimeSpan(0, 0, 0);  // DEFAULT '00:00:00'

        [Required]
        [Column("prodno")]
        public string ProdNo { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("dispensed")]
        public string Dispensed { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("prev_reading")]
        public int PrevReading { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("curr_reading")]
        public int CurrReading { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("consumption")]
        public int Consumption { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("memo")]
        public string Memo { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("mobilization")]
        public string Mobilization { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("room_id")]
        public string RoomId { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("date_start")]
        public DateTime DateStart { get; set; } = new DateTime(2001, 1, 1);  // NOT NULL, DEFAULT '2001-01-01'

        [Required]
        [Column("wtax_rate", TypeName = "decimal(12,2)")]
        public decimal WTaxRate { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("wtax_amount", TypeName = "decimal(12,2)")]
        public decimal WTaxAmount { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("block_no")]
        public string BlockNo { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("lot_no")]
        public string LotNo { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("interest_days")]
        public int InterestDays { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("daily_interest", TypeName = "decimal(12,2)")]
        public decimal DailyInterest { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("interest_rate", TypeName = "decimal(12,2)")]
        public decimal InterestRate { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("interest_period_start")]
        public int InterestPeriodStart { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("loan_no_of_payments")]
        public int LoanNoOfPayments { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("loan_payments_interval")]
        public int LoanPaymentsInterval { get; set; } = 0;  // NOT NULL, DEFAULT '0'

        [Required]
        [Column("loan_payments_term")]
        public string LoanPaymentsTerm { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("savings", TypeName = "decimal(12,2)")]
        public decimal Savings { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("cbu", TypeName = "decimal(12,2)")]
        public decimal Cbu { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("collector")]
        public string Collector { get; set; } = "";  // NOT NULL, DEFAULT ''

        [Required]
        [Column("insurance", TypeName = "decimal(12,2)")]
        public decimal Insurance { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("commission", TypeName = "decimal(12,2)")]
        public decimal Commission { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("collector_fee", TypeName = "decimal(12,2)")]
        public decimal CollectorFee { get; set; } = 0.00m;  // NOT NULL, DEFAULT '0.00'

        [Required]
        [Column("company_id")]
        public string CompanyId { get; set; } = "";  // NOT NULL, DEFAULT ''
    }
}
