using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Models
{
    public class Item
    {
        [Key]
        public long Id { get; set; }
        public string? EntryId { get; set; }
        public int? ItemQTY { get; set; }
        public decimal? ItemPrice { get; set; }
        public decimal? ItemSubTotal { get; set; }
        public bool IsVoid { get; set; } = false;
        public bool IsPwdDiscounted { get; set; } = false;
        public bool IsSeniorDiscounted { get; set; } = false;
        public bool IsTrainingMode { get; set; } = false;
        public Menu? Menu { get; set; }
        public Menu? Drink { get; set; }
        public Menu? AddOn { get; set; }
        public Item? Meal { get; set; }
        public required virtual Order Order { get; set; }

        public DateTimeOffset createdAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? VoidedAt { get; set; }
    }
}
