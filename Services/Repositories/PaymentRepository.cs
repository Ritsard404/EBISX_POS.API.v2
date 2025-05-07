using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Services.DTO.Payment;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBISX_POS.API.Services.Repositories
{
    public class PaymentRepository(DataContext _dataContext) : IPayment
    {
        public async Task<(bool, string)> AddAlternativePayments(List<AddAlternativePaymentsDTO> addAlternatives, string cashierEmail)
        {
            if (addAlternatives == null || !addAlternatives.Any())
            {
                return (false, "No Alternative Payments Provided");
            }

            var cashier = await _dataContext.User
                .FirstOrDefaultAsync(u => u.UserEmail == cashierEmail && u.IsActive);

            if (cashier == null)
            {
                return (false, "Invalid Cashier Credential");
            }

            var order = await _dataContext.Order
                .Where(o => o.Cashier == cashier && o.IsPending)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return (false, "No Pending Order");
            }

            var saleTypeIds = addAlternatives.Select(a => a.SaleTypeId).Distinct().ToList();

            // Fetch all the sale types and orders in bulk
            var checkSaleTypes = await _dataContext.SaleType
                .Where(st => saleTypeIds.Contains(st.Id))
                .ToListAsync();

            // Check for missing sale types
            var missingSaleTypes = saleTypeIds.Except(checkSaleTypes.Select(st => st.Id)).ToList();
            if (missingSaleTypes.Any())
            {
                return (false, $"SaleType(s) not found: {string.Join(", ", missingSaleTypes)}");
            }

            // Retrieve the relevant sale types for easy lookup
            var saleTypeLookup = checkSaleTypes.ToDictionary(st => st.Id, st => st);

            var alternativePayments = new List<AlternativePayments>();

            foreach (var alternative in addAlternatives)
            {

                if (!saleTypeLookup.TryGetValue(alternative.SaleTypeId, out var saleType))
                {
                    return (false, $"SaleType with ID {alternative.SaleTypeId} is missing.");
                }

                var newAlternativePayment = new AlternativePayments
                {
                    Reference = alternative.Reference,
                    Amount = alternative.Amount,
                    Order = order,
                    SaleType = saleType,
                };

                alternativePayments.Add(newAlternativePayment);
            }

            // Bulk insert the alternative payments
            await _dataContext.AlternativePayments.AddRangeAsync(alternativePayments);
            await _dataContext.SaveChangesAsync();

            return (true, "Alternative Payments Added Successfully");
        }

        public async Task<List<AlternativePayments>> GetAltPaymentsByOrderId(long orderId)
        {
            return await _dataContext.AlternativePayments
                .Include(a => a.SaleType)
                .Where(a => a.Order.Id == orderId)
                .ToListAsync();
        }

        public async Task<List<SaleType>> SaleTypes()
        {
            return await _dataContext.SaleType.ToListAsync();
        }
    }
}
