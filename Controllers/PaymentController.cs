using EBISX_POS.API.Models;
using EBISX_POS.API.Services.DTO.Payment;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBISX_POS.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PaymentController(IPayment _payment) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> SaleTypes()
        {
            var paymentMethods = await _payment.SaleTypes();
            return Ok(paymentMethods);
        }

        [HttpGet]
        public async Task<IActionResult> GetAltPaymentsByOrderId(long orderId)
        {
            var altPayments = await _payment.GetAltPaymentsByOrderId(orderId);
            return Ok(altPayments);
        }

        [HttpPost]
        public async Task<IActionResult> AddAlternativePayments(List<AddAlternativePaymentsDTO> addAlternatives, string cashierEmail)
        {
            var (success, message) = await _payment.AddAlternativePayments(addAlternatives, cashierEmail);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }
    }
}
