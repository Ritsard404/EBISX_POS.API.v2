using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EBISX_POS.API.Models.Utils;

namespace EBISX_POS.API.Models
{
    public class User
    {
        [Key]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string UserEmail { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public required string UserFName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public required string UserLName { get; set; }

        [Required(ErrorMessage = "User role is required")]
        public required string UserRole { get; set; }

        [JsonIgnore]
        public bool IsActive { get; set; } = true;
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [NotMapped]
        public string FullName => $"{UserFName} {UserLName}";

        [NotMapped]
        public string Status
        {
            get => IsActive ? "Active" : "Inactive";
            set => IsActive = string.Equals(value, "Active", StringComparison.OrdinalIgnoreCase);
        }
    }
}
