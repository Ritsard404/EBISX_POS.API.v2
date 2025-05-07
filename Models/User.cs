using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Models
{
    public class User
    {
        [Key]
        public required string UserEmail { get; set; }
        public required string UserFName { get; set; }
        public required string UserLName { get; set; }
        public required string UserRole { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
