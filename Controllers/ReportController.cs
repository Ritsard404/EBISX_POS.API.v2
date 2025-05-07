using EBISX_POS.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBISX_POS.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReportController(IReport _report) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> CashTrack(string cashierEmail)
        {
            var (CashInDrawer, CurrentCashDrawer) = await _report.CashTrack(cashierEmail);

            return Ok(new
            {
                CashInDrawer,
                CurrentCashDrawer
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoicesByDateRange(DateTime fromDate, DateTime toDate)
        {
            return Ok(await _report.GetInvoicesByDateRange(fromDate, toDate));
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceById(long invId)
        {
            return Ok(await _report.GetInvoiceById(invId));
        }

        [HttpGet]
        public async Task<IActionResult> XInvoiceReport()
        {
            return Ok(await _report.XInvoiceReport());
        }

        [HttpGet]
        public async Task<IActionResult> ZInvoiceReport()
        {
            return Ok(await _report.ZInvoiceReport());
        }

        [HttpGet]
        public async Task<IActionResult> UserActionLog(bool isManagerLog, DateTime fromDate, DateTime toDate)
        {
            return Ok(await _report.UserActionLog(isManagerLog, fromDate, toDate));
        }
    }
}
