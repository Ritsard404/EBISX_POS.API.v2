using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBISX_POS.API.Models
{
    public class Order
    {
        [Key]
        public long Id { get; set; }

        public required long InvoiceNumber { get; set; }

        public required string OrderType { get; set; }
        public required decimal TotalAmount { get; set; }
        public decimal? CashTendered { get; set; }
        public decimal? DueAmount { get; set; }
        public decimal? TotalTendered { get; set; }
        public decimal? ChangeAmount { get; set; }
        public decimal? VatSales { get; set; }
        public decimal? VatExempt { get; set; }
        public decimal? VatAmount { get; set; }
        public required DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? StatusChangeDate { get; set; }

        public bool IsCancelled { get; set; } = false;
        public bool IsReturned { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public bool IsTrainMode { get; set; } = false;
        public bool IsPending { get; set; } = true;

        public string? DiscountType { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public int? EligiblePwdScCount { get; set; }
        public string? EligibleDiscNames { get; set; }
        public string? OSCAIdsNum { get; set; }
        public int PrintCount { get; set; } = 0;
        public ICollection<CouponPromo> Coupon { get; set; } = new List<CouponPromo>();
        public CouponPromo? Promo { get; set; }

        public required User Cashier { get; set; }
        public ICollection<UserLog>? UserLog { get; set; } = new List<UserLog>();

        // Navigation property for related Items
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<AlternativePayments> AlternativePayments { get; set; } = new List<AlternativePayments>();
    }
}
