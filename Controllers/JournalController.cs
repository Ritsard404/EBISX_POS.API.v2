using EBISX_POS.API.Services.DTO.Journal;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBISX_POS.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalController(IJournal _journal) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> AccountJournals()
        {
            return Ok(await _journal.AccountJournals());
        }

        [HttpPost]
        public async Task<IActionResult> AddPwdScAccountJournal(AddPwdScAccountJournalDTO journalDTO)
        {
            var (success, message) = await _journal.AddPwdScAccountJournal(journalDTO);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpPut]
        public async Task<IActionResult> TruncateOrders()
        {
            var (success, message) = await _journal.TruncateOrders();
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        //[HttpPost]
        //public async Task<IActionResult> AddItemsJournal(long orderId)
        //{
        //    var (success, message) = await _journal.AddItemsJournal(orderId);
        //    if (success)
        //    {
        //        return Ok(message);
        //    }
        //    return BadRequest(message);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddTendersJournal(long orderId)
        //{
        //    var (success, message) = await _journal.AddTendersJournal(orderId);
        //    if (success)
        //    {
        //        return Ok(message);
        //    }
        //    return BadRequest(message);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddTotalsJournal(long orderId)
        //{
        //    var (success, message) = await _journal.AddTotalsJournal(orderId);
        //    if (success)
        //    {
        //        return Ok(message);
        //    }
        //    return BadRequest(message);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddPwdScJournal(long orderId)
        //{
        //    var (success, message) = await _journal.AddPwdScJournal(orderId);
        //    if (success)
        //    {
        //        return Ok(message);
        //    }
        //    return BadRequest(message);
        //}
    }
}
