using System.Text.Json.Serialization;

namespace EBISX_POS.API.Services.DTO.Order
{
    public class EditOrderItemQuantityDTO
    {
        public string entryId { get; set; }
        public int qty { get; set; }
        public required string CashierEmail { get; set; }
    }
}
