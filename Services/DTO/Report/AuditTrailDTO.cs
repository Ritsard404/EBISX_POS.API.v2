using System;

namespace EBISX_POS.API.Services.DTO.Report
{
    public class AuditTrailDTO
    {
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Amount { get; set; }
        public DateTime SortDateTime { get; set; } // For sorting purposes
    }
} 