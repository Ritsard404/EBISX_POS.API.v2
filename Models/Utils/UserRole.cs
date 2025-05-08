using System.ComponentModel;

namespace EBISX_POS.API.Models.Utils
{
    public enum UserRole
    {
        [Description("Cashier")]
        Cashier,

        [Description("Manager")]
        Manager
    }
}