using EBISX_POS.API.Services.DTO.Order;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBISX_POS.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController(IOrder _order) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddCurrentOrderVoid(AddCurrentOrderVoidDTO voidOrder)
        {
            var (success, message) = await _order.AddCurrentOrderVoid(voidOrder);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrderItem(AddOrderDTO addOrder)
        {
            var (success, message) = await _order.AddOrderItem(addOrder);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> VoidOrderItem(VoidOrderItemDTO voidOrder)
        {
            voidOrder.managerEmail = "user1@example.com";

            var (success, message) = await _order.VoidOrderItem(voidOrder);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> EditQtyOrderItem(EditOrderItemQuantityDTO editOrder)
        {
            var (success, message) = await _order.EditQtyOrderItem(editOrder);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> CancelCurrentOrder(string cashierEmail, string managerEmail)
        {
            var (success, message) = await _order.CancelCurrentOrder(cashierEmail, managerEmail);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> RefundOrder(string managerEmail, long invoiceNumber)
        {
            var (success, message) = await _order.RefundOrder(managerEmail, invoiceNumber);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> AddPwdScDiscount(AddPwdScDiscountDTO addPwdScDiscount)
        {
            var (success, message) = await _order.AddPwdScDiscount(addPwdScDiscount);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> AddOtherDiscount(AddOtherDiscountDTO addOtherDiscount)
        {
            var (success, message) = await _order.AddOtherDiscount(addOtherDiscount);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> PromoDiscount(string cashierEmail, string managerEmail, string promoCode)
        {

            var (success, message) = await _order.PromoDiscount(cashierEmail: cashierEmail, managerEmail: managerEmail, promoCode: promoCode);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> AvailCoupon(string cashierEmail, string managerEmail, string couponCode)
        {
            var (success, message) = await _order.AvailCoupon(cashierEmail: cashierEmail, managerEmail: managerEmail, couponCode: couponCode);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> FinalizeOrder(FinalizeOrderDTO finalizeOrder)
        {
            var (success, message, response) = await _order.FinalizeOrder(finalizeOrder);

            if (success)
            {
                // Return OK with the response DTO when success is true
                return Ok(response);
            }

            // Return BadRequest with the message when success is false
            return BadRequest(message);
        }


        [HttpGet]
        public async Task<IActionResult> GetCurrentOrderItems(string cashierEmail)
        {
            var currentOrderItems = await _order.GetCurrentOrderItems(cashierEmail);

            // Return the list (empty if no items found)
            return Ok(currentOrderItems);
        }

        [HttpGet]
        public async Task<IActionResult> GetElligiblePWDSCDiscount(string cashierEmail)
        {
            var elligiblePwdScDisc = await _order.GetElligiblePWDSCDiscount(cashierEmail);

            // Return the list (empty if no items found)
            return Ok(elligiblePwdScDisc);
        }

    }
}
