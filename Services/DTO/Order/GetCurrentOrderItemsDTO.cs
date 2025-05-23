namespace EBISX_POS.API.Services.DTO.Order
{
    public class GetCurrentOrderItemsDTO
    {
        // List of sub-orders (individual items in the order)
        public string EntryId { get; set; }
        public List<CurrentOrderItemsSubOrder>? SubOrders { get; set; }

        // Additional properties to display order summary (if needed)
        public int TotalQuantity => SubOrders?.FirstOrDefault()?.Quantity ?? 0;
        public decimal TotalPrice => SubOrders?
            .Where(i => !(i.AddOnId == null && i.MenuId == null && i.DrinkId == null) || CouponCode != null)
            .Sum(s => s.ItemSubTotal) ?? 0;
        public bool HasCurrentOrder => SubOrders != null && SubOrders.Any();
        public bool HasDiscount { get; set; } = false;
        public decimal? PromoDiscountAmount { get; set; }
        public bool IsPwdDiscounted { get; set; } = false;
        public bool IsSeniorDiscounted { get; set; } = false;
        public string? CouponCode { get; set; }
        public decimal DiscountAmount => SubOrders?
            .Where(i => (i.AddOnId == null && i.MenuId == null && i.DrinkId == null))
            .Sum(s => s.ItemSubTotal) ?? 0;
    }

    public class CurrentOrderItemsSubOrder
    {
        // Unique item identifiers
        public int? MenuId { get; set; }
        public int? DrinkId { get; set; }
        public int? AddOnId { get; set; }

        // Item details
        public string Name { get; set; } = string.Empty;
        public string? Size { get; set; }
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsFirstItem { get; set; } = false;
        public bool IsOtherDisc { get; set; } = false;

        // ✅ Computed properties
        public decimal ItemSubTotal => AddOnId == null && MenuId == null && DrinkId == null
            ? ItemPrice:
            ItemPrice * Quantity;

        public string DisplayName
        {
            get
            {
                // If size is null or whitespace, safeSize will be null.
                var safeSize = string.IsNullOrWhiteSpace(Size) ? null : Size.Trim();

                // If no IDs are set, return just the name.
                if (MenuId == null && DrinkId == null && AddOnId == null)
                {
                    return Name;
                }

                // If item has a positive price:
                if (ItemPrice > 0)
                {
                    // If there's a valid size, include it; otherwise, omit the size.
                    return string.IsNullOrEmpty(safeSize)
                        ? $"{Name} @{ItemPrice:G29}"
                        : $"{Name} ({safeSize}) @{ItemPrice:G29}";
                }

                // Fallback: if no price is present, return name with size (if available).
                return string.IsNullOrEmpty(safeSize)
                    ? Name
                    : $"{Name} ({safeSize})";
            }
        }

        public bool IsUpgradeMeal => ItemPrice > 0;

        public string ItemPriceString =>
     // 0) if the item’s price (or its subtotal) is zero, show nothing:
     (ItemPrice == 0m || ItemSubTotal == 0m)
         ? string.Empty
         // 1) explicit “other” → percent
         : IsOtherDisc
             ? $"{ItemPrice:0}%"
             // 2) pure‑discount & not “other” → negative ₱ (only if not zero)
             : (MenuId == null && DrinkId == null && AddOnId == null)
                 ? $" ₱{ItemSubTotal:G29}"
                 // 3) first item → positive ₱
                 : IsFirstItem
                     ? $"₱{ItemSubTotal:G29}"
                     // 4) upgrade → +₱ (only if ItemPrice > 0)
                     : IsUpgradeMeal
                         ? $"+ ₱{ItemSubTotal:G29}"
                         // 5) otherwise (i.e. a non‐first, non‐upgrade) → – ₱
                         : $"- ₱{ItemSubTotal:G29}";                              // 5) catch‑all → -₱



        // ✅ Opacity property for UI handling (optional)
        public double Opacity => IsFirstItem ? 1.0 : 0.0;
    }
}
