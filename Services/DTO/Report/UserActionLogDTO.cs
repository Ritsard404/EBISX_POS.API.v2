
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EBISX_POS.API.Services.DTO.Report
{
    public class UserActionLogDTO
    {
        public string? Name { get; set; }
        public string? ManagerEmail { get; set; }
        public string? CashierName { get; set; }
        public string? CashierEmail { get; set; }
        public string? Amount { get; set; }

        public required string Action { get; set; }
        public required string ActionDate { get; set; }
        [JsonIgnore]
        public DateTime SortActionDate { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ManagerActionType
    {
        Login,
        Logout,
        Edit,
        Cancel,
        Refund,
        Discount,
        Void,
        [EnumMember(Value = "Set Cash In Drawer")]
        SetCashInDrawer,
        [EnumMember(Value = "Set Cash Out Drawer")]
        SetCashOutDrawer,
        [EnumMember(Value = "Cash WithDraw")]
        CashWithdraw,
        ZReport,

    }
}
