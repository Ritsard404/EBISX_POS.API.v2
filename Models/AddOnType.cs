using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Models
{
    public class AddOnType
    {
        [Key]
        public int Id { get; set; }
        public required string AddOnTypeName { get; set; }
    }
}
