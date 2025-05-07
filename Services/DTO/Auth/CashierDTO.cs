using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Services.DTO.Auth
{
    public class CashierDTO
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
    }
}
