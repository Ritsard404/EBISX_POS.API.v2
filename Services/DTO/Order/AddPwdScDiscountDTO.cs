using EBISX_POS.API.Services.DTO.Journal;
using System.Text.Json.Serialization;

namespace EBISX_POS.API.Services.DTO.Order
{
    public class AddPwdScDiscountDTO
    {
        public int PwdScCount { get; set; }
        public bool IsSeniorDisc { get; set; }
        public required string EligiblePwdScNames { get; set; }
        public required string OSCAIdsNum { get; set; }
        public required string ManagerEmail { get; set; }
        public required string CashierEmail { get; set; }
        public required List<PwdScInfoDTO> PwdScInfo { get; set; }
        public required List<string> EntryId { get; set; }
    }
}
