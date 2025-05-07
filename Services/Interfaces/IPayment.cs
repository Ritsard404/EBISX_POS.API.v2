using EBISX_POS.API.Models;
using EBISX_POS.API.Services.DTO.Payment;

namespace EBISX_POS.API.Services.Interfaces
{
    public interface IPayment
    {
        Task<List<SaleType>> SaleTypes(); 
        Task<List<AlternativePayments>> GetAltPaymentsByOrderId(long orderId);


        Task<(bool, string)> AddAlternativePayments(List<AddAlternativePaymentsDTO> addAlternatives, string cashierEmail);
    }
}
