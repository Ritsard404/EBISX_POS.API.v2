using EBISX_POS.API.Services.DTO.Report;

namespace EBISX_POS.API.Services.Interfaces
{
    public interface IReport
    {
        Task<(string CashInDrawer, string CurrentCashDrawer)> CashTrack(string cashierEmail);
        Task<List<GetInvoicesDTO>> GetInvoicesByDateRange(DateTime fromDate, DateTime toDate);
        Task<GetInvoiceDTO> GetInvoiceById(long invId);
        Task<XInvoiceReportDTO> XInvoiceReport();
        Task<ZInvoiceReportDTO> ZInvoiceReport();
        Task<List<UserActionLogDTO>> UserActionLog(bool isManagerLog, DateTime fromDate, DateTime toDate);
    }
}
