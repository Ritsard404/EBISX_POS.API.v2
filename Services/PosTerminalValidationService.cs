using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBISX_POS.API.Services
{
    public class PosTerminalValidationService : IPosTerminalValidationService
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<PosTerminalValidationService> _logger;

        public PosTerminalValidationService(DataContext dataContext, ILogger<PosTerminalValidationService> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<(bool IsValid, string Message)> ValidateTerminalExpiration()
        {
            var terminalInfo = await GetTerminalInfo();
            if (terminalInfo == null)
            {
                return (false, "POS terminal is not configured.");
            }

            if (await IsTerminalExpired())
            {
                return (false, "POS terminal has expired. Please contact your administrator.");
            }

            if (await IsTerminalExpiringSoon())
            {
                return (true, "Warning: POS terminal will expire soon. Please contact your administrator.");
            }

            return (true, "POS terminal is valid.");
        }

        public async Task<bool> IsTerminalExpired()
        {
            var terminalInfo = await GetTerminalInfo();
            if (terminalInfo == null)
            {
                return true;
            }

            return DateTime.Now > terminalInfo.ValidUntil;
        }

        public async Task<bool> IsTerminalExpiringSoon()
        {
            var terminalInfo = await GetTerminalInfo();
            if (terminalInfo == null)
            {
                return false;
            }

            var oneWeekFromNow = DateTime.Now.AddDays(7);
            return DateTime.Now <= terminalInfo.ValidUntil && terminalInfo.ValidUntil <= oneWeekFromNow;
        }

        public async Task<PosTerminalInfo?> GetTerminalInfo()
        {
            try
            {
                return await _dataContext.PosTerminalInfo
                    .AsNoTracking()
                    .SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving POS terminal info");
                return null;
            }
        }
    }
} 