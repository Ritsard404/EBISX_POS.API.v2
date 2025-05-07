namespace EBISX_POS.API.Services.DTO.Order
{
    public class AddOtherDiscountDTO
    {

        public required string DiscountName { get; set; }
        public required int DiscPercent { get; set; }
        public required string ManagerEmail { get; set; }
        public required string CashierEmail { get; set; }
    }
}
