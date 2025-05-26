using EBISX_POS.API.Services.Interfaces;
using EBISX_POS.API.Services.PDF;
using EBISX_POS.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EBISX_POS.API.Models;
using System.IO;

namespace EBISX_POS.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReport _report;
        private readonly DataContext _dataContext;
        private readonly string _pdfStoragePath;

        public ReportController(IReport report, DataContext dataContext, IConfiguration configuration)
        {
            _report = report;
            _dataContext = dataContext;
            // Get the PDF storage path from configuration or use a default path
            _pdfStoragePath = configuration["PDFStorage:Path"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PDFs");
            
            // Ensure the directory exists
            if (!Directory.Exists(_pdfStoragePath))
            {
                Directory.CreateDirectory(_pdfStoragePath);
            }
        }

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
        [HttpGet]
        public async Task<IActionResult> GetAuditTrail(DateTime fromDate, DateTime toDate)
        {
            return Ok(await _report.GetAuditTrail(fromDate, toDate));
        }

        [HttpGet("audit-trail-pdf")]
        public async Task<IActionResult> GetAuditTrailPDF(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Get audit trail data
                var auditTrail = await _report.GetAuditTrail(fromDate, toDate);

                // Get POS terminal info
                var posInfo = await _dataContext.PosTerminalInfo.FirstOrDefaultAsync() ?? new PosTerminalInfo
                {
                    RegisteredName = "N/A",
                    OperatedBy = "N/A",
                    Address = "N/A",
                    VatTinNumber = "N/A",
                    MinNumber = "N/A",
                    PosSerialNumber = "N/A",
                    AccreditationNumber = "N/A",
                    DateIssued = DateTime.Now,
                    PtuNumber = "N/A",
                    ValidUntil = DateTime.Now
                };

                // Generate PDF
                var pdfService = new AuditTrailPDFService(
                    posInfo.RegisteredName,
                    posInfo.Address,
                    posInfo.VatTinNumber,
                    posInfo.MinNumber,
                    posInfo.PosSerialNumber
                );

                var pdfBytes = pdfService.GenerateAuditTrailPDF(auditTrail, fromDate, toDate);
                var fileName = $"AuditTrail_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                var filePath = Path.Combine(_pdfStoragePath, fileName);

                // Save the PDF to the local directory
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                // Return both the file path and a success message
                return Ok(new 
                { 
                    message = "PDF generated and saved successfully",
                    filePath = filePath,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
