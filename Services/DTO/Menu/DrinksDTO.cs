namespace EBISX_POS.API.Services.DTO.Menu
{
    public class DrinksDTO
    {
        public int MenuId { get; set; }
        public required string MenuName { get; set; }
        public string? MenuImagePath { get; set; }
        public decimal MenuPrice { get; set; }

        // Add the Size property so the upgrade logic can be computed.
        public string? Size { get; set; }

        // Computed property: if Size is null or empty, or not "R", then it's an upgrade meal.
        public bool IsUpgradeMeal => string.IsNullOrEmpty(Size) || !string.Equals(Size, "R", StringComparison.OrdinalIgnoreCase);

    }

    public class SizesWithPricesDTO
    {
        public string Size { get; set; }
        //public decimal Price { get; set; }
        public List<DrinksDTO>? Drinks { get; set; }
    }
}
