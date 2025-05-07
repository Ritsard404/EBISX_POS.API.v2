
using EBISX_POS.API.Models;

namespace EBISX_POS.API.Services.Interfaces
{
    public interface IEbisxAPI
    {
        Task<(bool, string)> FetchUsers();
        Task<(bool, string)> FetchSaleTypes();
        Task<(bool IsSuccess, string Message)> SetPosTerminalInfo(PosTerminalInfo posTerminalInfo);
        Task<PosTerminalInfo> PosTerminalInfo();

    }
}
