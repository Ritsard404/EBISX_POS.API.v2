using EBISX_POS.API.Services.DTO.Report;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Globalization;
using System.Text;

namespace EBISX_POS.API.Services.PDF
{
    public class TransactionListPDFService
    {
        static TransactionListPDFService()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private string _businessName;
        private string _address;
        private string _tin;

        public TransactionListPDFService()
        {
            // Initialize with default values
            _businessName = "N/A";
            _address = "N/A";
            _tin = "N/A";
        }

        public void UpdateBusinessInfo(string businessName, string address, string tin)
        {
            _businessName = businessName;
            _address = address;
            _tin = tin;
        }

        public byte[] GenerateTransactionListPDF(List<TransactionListDTO> transactions, DateTime fromDate, DateTime toDate)
        {
            using var document = new PdfDocument();
            var page = document.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Landscape;
            // Set page size to 8.5 x 13 inches (long bond paper)
            page.Width = XUnit.FromInch(13.0);
            page.Height = XUnit.FromInch(8.5);
            var gfx = XGraphics.FromPdfPage(page);
            var phCulture = new CultureInfo("en-PH");

            // Fonts
            var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 10, XFontStyle.Bold);
            var normalFont = new XFont("Arial", 9, XFontStyle.Regular);
            var smallFont = new XFont("Arial", 8, XFontStyle.Regular);

            double y = 40;
            double margin = 30;
            double tableTop = 0;
            double pageWidth = page.Width - margin * 2;

            // Header
            gfx.DrawString(_businessName, titleFont, XBrushes.DarkBlue, new XPoint(margin, y));
            y += 18;
            gfx.DrawString(_address, normalFont, XBrushes.Black, new XPoint(margin, y));
            y += 12;
            gfx.DrawString($"TIN {_tin}", normalFont, XBrushes.Black, new XPoint(margin, y));
            y += 18;
            gfx.DrawString("DAILY TRANSACTION LIST", headerFont, XBrushes.DarkBlue, new XPoint(margin, y));
            y += 14;
            gfx.DrawString($"From {fromDate:MM-dd-yyyy} To {toDate:MM-dd-yyyy}", normalFont, XBrushes.Black, new XPoint(margin, y));
            y += 18;
            tableTop = y;

            // Table columns (sum of fractions = 1.0)
            var columns = new[]
            {
                ("DATE", 0.07),
                ("OR NO", 0.10),
                ("SRC", 0.07),
                ("DISC\nTYPE", 0.07),
                ("%", 0.03),
                ("SUB\nTOTAL", 0.06),
                ("AMOUNT\nDUE", 0.06),
                ("GROSS\nSALES", 0.06),
                ("RETURNS", 0.06),
                ("NET OF\nRETURNS", 0.06),
                ("LESS\nDISCOUNT", 0.06),
                ("NET OF\nSALES", 0.06),
                ("VATABLE", 0.06),
                ("ZERO\nRATED", 0.06),
                ("EXEMPT", 0.06),
                ("VAT", 0.06)
            };
            double[] colWidths = columns.Select(c => c.Item2 * pageWidth).ToArray();
            double headerRowHeight = 30;
            double rowHeight = 18;

            var formats = new[]
            {
                    XStringFormats.Center,
                    XStringFormats.CenterLeft,
                    XStringFormats.Center,
                    XStringFormats.CenterLeft,
                    XStringFormats.Center,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight,
                    XStringFormats.CenterRight
                };

            // Draw table header
            double headerY = y;
            double x = margin;
            for (int i = 0; i < columns.Length; i++)
            {
                var rect = new XRect(x, headerY, colWidths[i], headerRowHeight);
                gfx.DrawRectangle(XBrushes.LightGray, rect);
                var headerLines = columns[i].Item1.Split('\n');
                double lineHeight = headerRowHeight / headerLines.Length;
                for (int j = 0; j < headerLines.Length; j++)
                {
                    var lineRect = new XRect(x, headerY + j * lineHeight, colWidths[i], lineHeight);
                    gfx.DrawString(headerLines[j], smallFont, XBrushes.Black, lineRect, formats[i]);
                }
                x += colWidths[i];
            }
            y += headerRowHeight;

            // Table rows
            foreach (var t in transactions)
            {
                x = margin;
                var values = new[]
                {
                    t.Date,
                    t.InvoiceNum,
                    t.Src,
                    t.DiscType,
                    t.Percent,
                    t.SubTotal.ToString("N2", phCulture),
                    t.AmountDue.ToString("N2", phCulture),
                    t.GrossSales.ToString("N2", phCulture),
                    t.Returns.ToString("N2", phCulture),
                    t.NetOfReturns.ToString("N2", phCulture),
                    t.LessDiscount.ToString("N2", phCulture),
                    t.NetOfSales.ToString("N2", phCulture),
                    t.Vatable.ToString("N2", phCulture),
                    t.ZeroRated.ToString("N2", phCulture),
                    t.Exempt.ToString("N2", phCulture),
                    t.Vat.ToString("N2", phCulture)
                };
                for (int i = 0; i < values.Length; i++)
                {
                    var rect = new XRect(x, y, colWidths[i], rowHeight);
                    gfx.DrawString(values[i], smallFont, XBrushes.Black, rect, formats[i]);
                    x += colWidths[i];
                }
                y += rowHeight;
                // Draw row line
                gfx.DrawLine(XPens.Gray, margin, y, margin + pageWidth, y); 
            }

            // Totals row
            x = margin;
            var totals = new[]
            {
                "", "", "", "", "", "", "TOTALS:",
                transactions.Sum(t => t.GrossSales).ToString("N2", phCulture),
                transactions.Sum(t => t.Returns).ToString("N2", phCulture),
                transactions.Sum(t => t.NetOfReturns).ToString("N2", phCulture),
                transactions.Sum(t => t.LessDiscount).ToString("N2", phCulture),
                transactions.Sum(t => t.NetOfSales).ToString("N2", phCulture),
                transactions.Sum(t => t.Vatable).ToString("N2", phCulture),
                transactions.Sum(t => t.ZeroRated).ToString("N2", phCulture),
                transactions.Sum(t => t.Exempt).ToString("N2", phCulture),
                transactions.Sum(t => t.Vat).ToString("N2", phCulture)
            };
            for (int i = 0; i < totals.Length; i++)
            {
                var rect = new XRect(x, y, colWidths[i], rowHeight);
                gfx.DrawRectangle(XBrushes.White, rect);
                gfx.DrawString(totals[i], headerFont, XBrushes.Black, rect, XStringFormats.CenterRight);
                x += colWidths[i];
            }
            y += rowHeight;

            // Draw debug rectangle for table area (optional, remove if not needed)
            // gfx.DrawRectangle(XPens.Red, margin, tableTop, pageWidth, y - tableTop);

            // Save to memory stream
            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }
    }
}
