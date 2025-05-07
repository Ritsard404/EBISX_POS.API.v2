using System.Collections.Generic;

namespace EBISX_POS.API.Services.DTO.Menu
{
    public class DrinkTypeWithDrinksDTO
    {
        public int DrinkTypeId { get; set; }
        public string DrinkTypeName { get; set; }
        public List<SizesWithPricesDTO>? SizesWithPrices { get; set; } = new List<SizesWithPricesDTO>();

    }
}
