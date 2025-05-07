namespace EBISX_POS.API.Services.DTO.Menu
{
    public class AddOnTypeDTO
    {
        public int MenuId { get; set; }
        public required string MenuName { get; set; }
        public string? MenuImagePath { get; set; }
        public string? Size { get; set; }
        public decimal? Price { get; set; }

        // If Size is not null or empty and is not "R", then it's considered an upgrade meal.
        public bool IsUpgradeMeal => string.IsNullOrEmpty(Size) || !string.Equals(Size, "R", StringComparison.OrdinalIgnoreCase);

    }
}
