using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBISX_POS.API.Models.Journal
{
    [Table("salesjournal")]
    public class SalesJournal
    {
        [Key]
        [Column("TransNo")]
        public long TransNo { get; set; }

        [Column("sale_type")]
        public string? SaleType { get; set; }

        [Column("TransDate")]
        public DateTime? TransDate { get; set; }

        [Column("DueDate")]
        public DateTime? DueDate { get; set; }

        [Column("costcenter")]
        public string? CostCenter { get; set; }

        [Column("Reference")]
        public string? Reference { get; set; }

        [Column("docref")]
        public string? DocRef { get; set; }

        [Column("externalref")]
        public string? ExternalRef { get; set; }

        [Column("Customer")]
        public string? Customer { get; set; }

        [Column("CustomerName")]
        public string? CustomerName { get; set; }

        [Column("customergroup")]
        public string? CustomerGroup { get; set; }

        [Column("Branch")]
        public string? Branch { get; set; }

        [Column("SubTotal")]
        public double? SubTotal { get; set; }

        [Column("TotalTax")]
        public double? TotalTax { get; set; }

        [Column("GrossTotal")]
        public double? GrossTotal { get; set; }

        [Column("Status")]
        public string? Status { get; set; }

        [Column("job_no")]
        public string? JobNo { get; set; }

        [Column("job_desc")]
        public string? JobDesc { get; set; }

        [Column("reprint_count")]
        public int? ReprintCount { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("delivery_address")]
        public string? DeliveryAddress { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("voyage")]
        public string? Voyage { get; set; }

        [Column("passenger")]
        public string? Passenger { get; set; }

        [Column("age")]
        public string? Age { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("agent")]
        public string? Agent { get; set; }

        [Column("delivery_date")]
        public DateTime? DeliveryDate { get; set; }

        [Column("emp_driver")]
        public string? EmpDriver { get; set; }

        [Column("approved_by")]
        public string? ApprovedBy { get; set; }

        [Column("approve_date")]
        public DateTime? ApproveDate { get; set; }

        [Column("order_time")]
        public TimeSpan? OrderTime { get; set; }

        [Column("post_time")]
        public TimeSpan? PostTime { get; set; }

        [Column("discount_type")]
        public string? DiscountType { get; set; }

        [Column("discount_amount", TypeName = "decimal(12,2)")]
        public decimal? DiscountAmount { get; set; }

        [Column("tender_type")]
        public string? TenderType { get; set; }

        [Column("customer_po")]
        public string? CustomerPO { get; set; }

        [Column("dine_take")]
        public string? DineTake { get; set; } // Default is 'DINE' if not provided

        // NOT NULL columns with defaults become non-nullable in the model:
        [Required]
        [Column("dispensed")]
        public string Dispensed { get; set; } = string.Empty;

        [Required]
        [Column("tender_reference")]
        public string TenderReference { get; set; } = string.Empty;

        [Required]
        [Column("contact_person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [Column("contact_number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [Column("inv_vat_rate", TypeName = "decimal(12,2)")]
        public decimal InvVatRate { get; set; }

        [Required]
        [Column("sale_tax_method")]
        public string SaleTaxMethod { get; set; } = string.Empty;

        [Required]
        [Column("inv_tax_id")]
        public string InvTaxId { get; set; } = string.Empty;

        [Required]
        [Column("inv_tax_type")]
        public string InvTaxType { get; set; } = "Inclusive";

        [Required]
        [Column("wtaxaccount")]
        public string WTaxAccount { get; set; } = string.Empty;

        [Required]
        [Column("wtaxrate", TypeName = "decimal(12,2)")]
        public decimal WTaxRate { get; set; }

        [Required]
        [Column("wtaxamt", TypeName = "decimal(12,2)")]
        public decimal WTaxAmt { get; set; }

        [Required]
        [Column("netpayable", TypeName = "decimal(12,2)")]
        public decimal NetPayable { get; set; }

        [Required]
        [Column("imagelocation")]
        public string ImageLocation { get; set; } = string.Empty;

        [Required]
        [Column("invoice_type")]
        public string InvoiceType { get; set; } = string.Empty;

        [Required]
        [Column("collector")]
        public string Collector { get; set; } = string.Empty;

        [Required]
        [Column("last_payment_date")]
        public DateTime LastPaymentDate { get; set; }

        [Required]
        [Column("no_of_payments")]
        public int NoOfPayments { get; set; }

        [Column("firstname")]
        public string? FirstName { get; set; }

        [Column("lastname")]
        public string? LastName { get; set; }

        [Column("charge_account")]
        public string? ChargeAccount { get; set; }

        [Column("beneficiary_bday")]
        public DateTime? BeneficiaryBday { get; set; }

        [Column("beneficiary_relationship")]
        public string? BeneficiaryRelationship { get; set; }

        [Column("beneficiary_gender")]
        public string? BeneficiaryGender { get; set; }

        [Column("beneficiary_contact")]
        public string? BeneficiaryContact { get; set; }

        [Column("assign_firstname")]
        public string? AssignFirstName { get; set; }

        [Required]
        [Column("assign_lastname")]
        public string AssignLastName { get; set; } = string.Empty;

        [Column("assign_date")]
        public DateTime? AssignDate { get; set; }

        [Column("claim_date")]
        public DateTime? ClaimDate { get; set; }

        [Column("decease_status")]
        public string? DeceaseStatus { get; set; }

        [Column("decease_date")]
        public DateTime? DeceaseDate { get; set; }

        [Required]
        [Column("company_id")]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [Column("vatsales", TypeName = "decimal(12,2)")]
        public decimal VatSales { get; set; }

        [Required]
        [Column("zerosales", TypeName = "decimal(12,2)")]
        public decimal ZeroSales { get; set; }

        [Required]
        [Column("exemptsales", TypeName = "decimal(12,2)")]
        public decimal ExemptSales { get; set; }

        [Required]
        [Column("zeroexempt")]
        public string ZeroExempt { get; set; } = string.Empty;

        [Required]
        [Column("payor_name")]
        public string PayorName { get; set; } = string.Empty;

        [Required]
        [Column("payor_address")]
        public string PayorAddress { get; set; } = string.Empty;

        [Required]
        [Column("payor_relationship")]
        public string PayorRelationship { get; set; } = string.Empty;

        [Required]
        [Column("payor_mobile")]
        public string PayorMobile { get; set; } = string.Empty;

        [Required]
        [Column("viewing_place")]
        public string ViewingPlace { get; set; } = string.Empty;

        [Required]
        [Column("interment_place")]
        public string IntermentPlace { get; set; } = string.Empty;

        [Required]
        [Column("contact_relationship")]
        public string ContactRelationship { get; set; } = string.Empty;

        [Required]
        [Column("deceased_age")]
        public string DeceasedAge { get; set; } = string.Empty;
    }
}
