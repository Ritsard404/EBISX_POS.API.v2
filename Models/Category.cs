using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public required string CtgryName { get; set; }
    }
}
