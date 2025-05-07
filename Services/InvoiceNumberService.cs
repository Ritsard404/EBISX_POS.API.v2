using EBISX_POS.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EBISX_POS.API.Services
{
    public interface IInvoiceNumberService
    {
        Task<long> GenerateInvoiceNumberAsync(bool isTrainingMode);
    }

    public class InvoiceNumberService : IInvoiceNumberService
    {
        private readonly DataContext _context;

        public InvoiceNumberService(DataContext context)
        {
            _context = context;
        }

        public async Task<long> GenerateInvoiceNumberAsync(bool isTrainingMode)
        {
            // Get the latest order number
            var latestOrder = await _context.Order
                .Where(o => o.IsTrainMode == isTrainingMode)
                .OrderByDescending(o => o.InvoiceNumber)
                .FirstOrDefaultAsync();

            return latestOrder?.InvoiceNumber + 1 ?? 1;
        }
    }
}