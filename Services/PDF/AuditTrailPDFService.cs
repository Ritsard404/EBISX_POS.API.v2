using EBISX_POS.API.Services.DTO.Report;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Globalization;
using System.Text;

namespace EBISX_POS.API.Services.PDF
{
    public class AuditTrailPDFService
    {
        static AuditTrailPDFService()
        {
            // Register encoding provider for PDFsharp
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private string _businessName;
        private string _address;
        private string _vatTinNumber;
        private string _minNumber;
        private string _serialNumber;
        private PdfDocument _document;

        // Font settings
        private const string FONT_FAMILY = "Arial";
        private const double TITLE_FONT_SIZE = 16;
        private const double HEADER_FONT_SIZE = 12;
        private const double NORMAL_FONT_SIZE = 10;
        private const double SMALL_FONT_SIZE = 8;

        // Colors
        private static readonly XColor HEADER_COLOR = XColors.DarkBlue;
        private static readonly XColor TABLE_HEADER_COLOR = XColors.LightGray;
        private static readonly XColor BORDER_COLOR = XColors.Gray;

        // Margins and spacing
        private const double MARGIN = 40;
        private const double LINE_SPACING = 5;
        private const double TABLE_ROW_HEIGHT = 20;
        private const double TABLE_CELL_PADDING = 5;

        public AuditTrailPDFService()
        {
            // Initialize with default values
            _businessName = "N/A";
            _address = "N/A";
            _vatTinNumber = "N/A";
            _minNumber = "N/A";
            _serialNumber = "N/A";
        }

        public void UpdateBusinessInfo(string businessName, string address, string vatTinNumber, string minNumber, string serialNumber)
        {
            _businessName = businessName;
            _address = address;
            _vatTinNumber = vatTinNumber;
            _minNumber = minNumber;
            _serialNumber = serialNumber;
        }

        public byte[] GenerateAuditTrailPDF(List<AuditTrailDTO> auditTrail, DateTime fromDate, DateTime toDate)
        {
            // Create a new PDF document
            _document = new PdfDocument();
            var page = _document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var phCulture = new CultureInfo("en-PH");

            // Set up fonts with correct style values
            var titleFont = new XFont(FONT_FAMILY, TITLE_FONT_SIZE, XFontStyle.Bold);
            var headerFont = new XFont(FONT_FAMILY, HEADER_FONT_SIZE, XFontStyle.Bold);
            var normalFont = new XFont(FONT_FAMILY, NORMAL_FONT_SIZE, XFontStyle.Regular);
            var smallFont = new XFont(FONT_FAMILY, SMALL_FONT_SIZE, XFontStyle.Regular);

            double yPosition = MARGIN;

            // Draw header
            yPosition = DrawHeader(gfx, titleFont, normalFont, yPosition);

            // Draw report title and date range
            yPosition = DrawReportTitle(gfx, headerFont, normalFont, fromDate, toDate, phCulture, yPosition);

            // Draw table
            yPosition = DrawTable(gfx, normalFont, auditTrail, yPosition);

            // Draw footer
            DrawFooter(gfx, smallFont, page);

            // Save to memory stream
            using var stream = new MemoryStream();
            _document.Save(stream);
            return stream.ToArray();
        }

        private double DrawHeader(XGraphics gfx, XFont titleFont, XFont normalFont, double yPosition)
        {
            // Business Name
            gfx.DrawString(_businessName, titleFont, new XSolidBrush(HEADER_COLOR), new XPoint(MARGIN, yPosition));
            yPosition += TITLE_FONT_SIZE + LINE_SPACING;

            // Address and other details
            gfx.DrawString(_address, normalFont, XBrushes.Black, new XPoint(MARGIN, yPosition));
            yPosition += NORMAL_FONT_SIZE + LINE_SPACING;

            gfx.DrawString($"VAT TIN: {_vatTinNumber}", normalFont, XBrushes.Black, new XPoint(MARGIN, yPosition));
            yPosition += NORMAL_FONT_SIZE + LINE_SPACING;

            gfx.DrawString($"MIN: {_minNumber}", normalFont, XBrushes.Black, new XPoint(MARGIN, yPosition));
            yPosition += NORMAL_FONT_SIZE + LINE_SPACING;

            gfx.DrawString($"S/N: {_serialNumber}", normalFont, XBrushes.Black, new XPoint(MARGIN, yPosition));
            yPosition += NORMAL_FONT_SIZE + LINE_SPACING * 2;

            // Draw line
            gfx.DrawLine(new XPen(BORDER_COLOR), new XPoint(MARGIN, yPosition), new XPoint(gfx.PdfPage.Width - MARGIN, yPosition));
            yPosition += LINE_SPACING * 2;

            return yPosition;
        }

        private double DrawReportTitle(XGraphics gfx, XFont headerFont, XFont normalFont, DateTime fromDate, DateTime toDate, CultureInfo phCulture, double yPosition)
        {
            // Report Title
            gfx.DrawString("AUDIT TRAIL REPORT", headerFont, new XSolidBrush(HEADER_COLOR), new XPoint(MARGIN, yPosition));
            yPosition += HEADER_FONT_SIZE + LINE_SPACING;

            // Date Range
            var dateRange = $"From: {fromDate.ToString("MM/dd/yyyy", phCulture)} To: {toDate.ToString("MM/dd/yyyy", phCulture)}";
            gfx.DrawString(dateRange, normalFont, XBrushes.Black, new XPoint(MARGIN, yPosition));
            yPosition += NORMAL_FONT_SIZE + LINE_SPACING;

            // Generation Time
            var generationTime = $"Generated: {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt", phCulture)}";
            gfx.DrawString(generationTime, normalFont, XBrushes.Black, new XPoint(MARGIN, yPosition));
            yPosition += NORMAL_FONT_SIZE + LINE_SPACING * 2;

            return yPosition;
        }

        private double DrawTable(XGraphics gfx, XFont normalFont, List<AuditTrailDTO> auditTrail, double yPosition)
        {
            var pageWidth = gfx.PdfPage.Width - (MARGIN * 2);
            var columnWidths = new[]
            {
                pageWidth * 0.15, // Date
                pageWidth * 0.15, // Time
                pageWidth * 0.20, // User
                pageWidth * 0.30, // Action
                pageWidth * 0.20  // Amount
            };

            // Draw table header
            var headerBrush = new XSolidBrush(TABLE_HEADER_COLOR);
            var headerPen = new XPen(BORDER_COLOR);
            var currentX = MARGIN;

            // Draw header cells
            var headers = new[] { "Date", "Time", "User", "Action", "Amount" };
            for (int i = 0; i < headers.Length; i++)
            {
                var rect = new XRect(currentX, yPosition, columnWidths[i], TABLE_ROW_HEIGHT);
                gfx.DrawRectangle(headerBrush, rect);
                
                var textRect = new XRect(currentX + TABLE_CELL_PADDING, yPosition, columnWidths[i] - (TABLE_CELL_PADDING * 2), TABLE_ROW_HEIGHT);
                gfx.DrawString(headers[i], normalFont, XBrushes.Black, textRect, XStringFormats.Center);
                
                currentX += columnWidths[i];
            }
            yPosition += TABLE_ROW_HEIGHT;

            // Draw table rows
            var rowPen = new XPen(BORDER_COLOR);
            foreach (var entry in auditTrail)
            {
                // Check if we need a new page
                if (yPosition + TABLE_ROW_HEIGHT > gfx.PdfPage.Height - MARGIN)
                {
                    var newPage = _document.AddPage();
                    gfx = XGraphics.FromPdfPage(newPage);
                    yPosition = MARGIN;
                }

                currentX = MARGIN;
                var cells = new[] { entry.Date, entry.Time, entry.UserName, entry.Action, entry.Amount ?? "-" };
                var formats = new[] 
                { 
                    XStringFormats.Center, 
                    XStringFormats.Center, 
                    XStringFormats.CenterLeft, 
                    XStringFormats.CenterLeft, 
                    XStringFormats.Center 
                };

                // Draw row background (alternating colors for better readability)
                var rowRect = new XRect(MARGIN, yPosition, pageWidth, TABLE_ROW_HEIGHT);
                if (auditTrail.IndexOf(entry) % 2 == 0)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColors.White), rowRect);
                }
                else
                {
                    gfx.DrawRectangle(new XSolidBrush(XColors.WhiteSmoke), rowRect);
                }

                // Draw row bottom border
                gfx.DrawLine(rowPen, 
                    new XPoint(MARGIN, yPosition + TABLE_ROW_HEIGHT),
                    new XPoint(MARGIN + pageWidth, yPosition + TABLE_ROW_HEIGHT));

                // Draw cell contents
                for (int i = 0; i < cells.Length; i++)
                {
                    var textRect = new XRect(currentX + TABLE_CELL_PADDING, yPosition, columnWidths[i] - (TABLE_CELL_PADDING * 2), TABLE_ROW_HEIGHT);
                    gfx.DrawString(cells[i], normalFont, XBrushes.Black, textRect, formats[i]);
                    currentX += columnWidths[i];
                }
                yPosition += TABLE_ROW_HEIGHT;
            }

            return yPosition;
        }

        private void DrawFooter(XGraphics gfx, XFont font, PdfPage page)
        {
            // Get page number by finding the index in the document's pages collection
            int pageNumber = 1;
            for (int i = 0; i < _document.Pages.Count; i++)
            {
                if (_document.Pages[i] == page)
                {
                    pageNumber = i + 1;
                    break;
                }
            }

            var totalPages = _document.PageCount;
            var footerText = $"Page {pageNumber} of {totalPages}";
            var footerRect = new XRect(MARGIN, page.Height - MARGIN, page.Width - (MARGIN * 2), MARGIN);
            
            // Draw page numbers
            gfx.DrawString(footerText, font, XBrushes.Black, footerRect, XStringFormats.BottomLeft);
            
            // Draw system name
            gfx.DrawString("EBISX POS System", font, XBrushes.Black, footerRect, XStringFormats.BottomRight);
        }
    }
} 