using EBISX_POS.API.Models;

namespace EBISX_POS.API.Services.Interfaces
{
    public interface IPosTerminalValidationService
    {
        Task<(bool IsValid, string Message)> ValidateTerminalExpiration();
        Task<bool> IsTerminalExpired();
        Task<bool> IsTerminalExpiringSoon();
        Task<PosTerminalInfo?> GetTerminalInfo();
    }
} 