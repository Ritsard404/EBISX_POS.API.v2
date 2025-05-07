using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBISX_POS.API.Services.Repositories
{
    public class EbisxAPIRepository(DataContext _dataContext, ILogger<EbisxAPIRepository> _logger) : IEbisxAPI
    {
        public Task<(bool, string)> FetchSaleTypes()
        {
            throw new NotImplementedException();
        }

        public async Task<(bool IsSuccess, string Message)> SetPosTerminalInfo(PosTerminalInfo posTerminalInfo)
        {
            if (posTerminalInfo is null)
                return (false, "Terminal info cannot be null.");

            try
            {
                // Load the one-and-only record (or null)
                var existing = await _dataContext.PosTerminalInfo
                                                 .AsTracking()
                                                 .SingleOrDefaultAsync();

                if (existing == null)
                {
                    // No record yet: insert new
                    await _dataContext.PosTerminalInfo.AddAsync(posTerminalInfo);
                    _logger.LogInformation("Creating POS terminal record (Serial={Serial})",
                                           posTerminalInfo.PosSerialNumber);
                }
                else
                {
                    // Update every field, including primary key if changed
                    _dataContext.Entry(existing).CurrentValues.SetValues(posTerminalInfo);
                    _logger.LogInformation("Updating POS terminal record (NewSerial={Serial})",
                                           posTerminalInfo.PosSerialNumber);
                }

                await _dataContext.SaveChangesAsync();
                return (true, "POS terminal info successfully saved.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DB error saving POS terminal info");
                return (false, "A database error occurred while saving terminal info.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving POS terminal info");
                return (false, "An unexpected error occurred.");
            }
        }
        public Task<(bool, string)> FetchUsers()
        {
            throw new NotImplementedException();
        }

        public async Task<PosTerminalInfo> PosTerminalInfo()
        {
            return await _dataContext.PosTerminalInfo.AsNoTracking().SingleOrDefaultAsync();
        }
    }
}
