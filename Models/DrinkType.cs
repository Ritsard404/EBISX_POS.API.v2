using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Models
{
    public class DrinkType
    {
        [Key]
        public int Id { get; set; }
        public required string DrinkTypeName { get; set; }
    }
}
