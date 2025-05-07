using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Models
{
    public class SaleType
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Account { get; set; }
        public required string Type { get; set; }
    }
}
