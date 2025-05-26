using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Models.Utils;
using EBISX_POS.API.Services.DTO.Order;
using EBISX_POS.API.Services.DTO.Report;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace EBISX_POS.API.Services.Repositories
{
    public class ReportRepository(DataContext _dataContext, IAuth _auth) : IReport
    {
        public async Task<(string CashInDrawer, string CurrentCashDrawer)> CashTrack(string cashierEmail)
        {
            // First get the timestamp
            var timestamp = await _dataContext.Timestamp
                .Include(t => t.Cashier)
                .Where(t => t.Cashier.UserEmail == cashierEmail && t.TsOut == null && t.CashInDrawerAmount != null && t.CashInDrawerAmount >= 1000)
                .FirstOrDefaultAsync();

            if (timestamp == null || timestamp.CashInDrawerAmount == null)
                return ("₱0.00", "₱0.00");

            var tsIn = timestamp.TsIn;

            // Fetch all orders with their cashier
            var orders = await _dataContext.Order
                .Include(o => o.Cashier)
                .ToListAsync();

            // Filter and calculate in memory
            decimal totalCashInDrawer = orders
                .Where(o =>
                    o.Cashier.UserEmail == cashierEmail &&
                    !o.IsCancelled &&
                    !o.IsPending &&
                    !o.IsReturned &&
                    o.CreatedAt >= tsIn &&
                    o.CashTendered != null &&
                    o.TotalAmount != 0)
                .Sum(o => o.CashTendered!.Value - o.ChangeAmount!.Value);

            // Get withdrawals
            var withdrawals = await _dataContext.UserLog
                .Where(u => u.Timestamp != null && u.Timestamp.Id == timestamp.Id && u.Action == "Cash Withdrawal")
                .ToListAsync();

            var totalWithdrawn = withdrawals.Sum(u => u.WithdrawAmount);

            var phCulture = new CultureInfo("en-PH");

            string cashInDrawerText = timestamp.CashInDrawerAmount.Value.ToString("C", phCulture);

            string currentCashDrawerText =
                (timestamp.CashInDrawerAmount.Value
                + totalCashInDrawer
                - totalWithdrawn
                ).ToString("C", phCulture);

            return (cashInDrawerText, currentCashDrawerText);
        }

        public async Task<List<GetInvoicesDTO>> GetInvoicesByDateRange(DateTime fromDate, DateTime toDate)
        {
            // normalize to midnight at the start of each day
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1);
            var isTrainMode = await _auth.IsTrainMode();

            // First fetch all orders with their related data
            var orders = await _dataContext.Order
                .Include(o => o.Cashier)
                .ToListAsync();

            var filteredOrders = orders
                .Where(o =>
                    o.CreatedAt >= start &&
                    o.CreatedAt < end &&
                    !o.IsPending &&
                    o.IsTrainMode == isTrainMode)
                .Select(s => new GetInvoicesDTO
                {
                    InvoiceNum = s.InvoiceNumber,
                    InvoiceNumString = s.InvoiceNumber.ToString("D12"),
                    Date = s.CreatedAt.ToString("MM/dd/yyyy"),
                    Time = s.CreatedAt.ToString("hh:mm tt"),
                    CashierName = s.Cashier.UserFName + " " + s.Cashier.UserLName,
                    CashierEmail = s.Cashier.UserEmail,
                    InvoiceStatus = s.IsCancelled ? "Cancelled" : s.IsReturned ? "Refund" : "Paid"
                })
                .OrderBy(i => i.InvoiceNum)
                .ToList();

            return filteredOrders;

        }


        public async Task<GetInvoiceDTO> GetInvoiceById(long invId)
        {
            var pesoCulture = new CultureInfo("en-PH");
            // 1) Load the order, its cashier, items and alternative payments
            var order = await _dataContext.Order
                .Include(o => o.Cashier)
                .Include(o => o.Items)
                .Include(o => o.AlternativePayments)
                    .ThenInclude(ap => ap.SaleType)
                .FirstOrDefaultAsync(o => o.InvoiceNumber == invId);

            if (order == null)
                return new GetInvoiceDTO();

            var orderItems = await GetOrderItems(order.Id);

            // 2) Load your POS terminal / business info (assumes a single row)
            var posInfo = await _dataContext.Set<PosTerminalInfo>()
                .FirstOrDefaultAsync();

            var discountValue = order.DiscountAmount ?? 0m;

            order.PrintCount += 1;

            await _dataContext.SaveChangesAsync();

            // 3) Map to your DTO
            var dto = new GetInvoiceDTO
            {
                // --- Business Details from POS info
                RegisteredName = posInfo?.RegisteredName ?? "",
                Address = posInfo?.Address ?? "",
                VatTinNumber = posInfo?.VatTinNumber ?? "",
                MinNumber = posInfo?.MinNumber ?? "",

                // --- Invoice Header
                InvoiceNum = order.InvoiceNumber.ToString("D12"),
                InvoiceDate = order.CreatedAt
                                          .ToString("MM/dd/yyyy HH:mm:ss"),
                OrderType = order.OrderType,
                CashierName = $"{order.Cashier.UserFName} {order.Cashier.UserLName}",

                // --- Line Items

                Items = orderItems
                .Select(group => new ItemDTO
                {
                    // take the quantity of the first (parent) sub‐order
                    Qty = group.TotalQuantity,

                    // map every sub‐order into your ItemInfoDTO
                    itemInfos = group.SubOrders?
                        .Select((s, index) => new ItemInfoDTO
                        {
                            IsFirstItem = index == 0,
                            Description = s.DisplayName,
                            Amount = s.ItemPriceString
                        })
                        .ToList()
                        // ensure non‐null list
                        ?? new List<ItemInfoDTO>()
                })
                .ToList(),



                // --- Totals
                TotalAmount = (order.TotalAmount).ToString("C", pesoCulture),
                DiscountAmount = discountValue != 0m
                      ? "-" + discountValue.ToString("C", pesoCulture)
                      : discountValue.ToString("C", pesoCulture),
                DueAmount = (order.DueAmount ?? 0m).ToString("C", pesoCulture),
                CashTenderAmount = (order.CashTendered ?? 0m).ToString("C", pesoCulture),
                TotalTenderAmount = (order.TotalTendered ?? 0m).ToString("C", pesoCulture),
                ChangeAmount = (order.ChangeAmount ?? 0m).ToString("C", pesoCulture),

                // VAT breakdown
                VatExemptSales = (order.VatExempt ?? 0m).ToString("C", pesoCulture),
                VatSales = ((order.TotalAmount - (order.VatAmount ?? 0m)))
                                      .ToString("C", pesoCulture),
                VatAmount = (order.VatAmount ?? 0m).ToString("C", pesoCulture),

                // Other tenders (e.g. gift cert, card, etc.)
                OtherPayments = order.AlternativePayments
                    .Select(ap => new OtherPaymentDTO
                    {
                        SaleTypeName = ap.SaleType.Name,
                        Amount = ap.Amount.ToString("C2")
                    })
                    .ToList(),

                // PWD/Senior/etc.
                ElligiblePeopleDiscounts = order.EligibleDiscNames?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList()
                ?? new List<string>(),

                // --- POS Details
                PosSerialNumber = posInfo?.PosSerialNumber ?? "",
                DateIssued = posInfo?.DateIssued.ToString("MM/dd/yyyy") ?? "",
                ValidUntil = posInfo?.ValidUntil.ToString("MM/dd/yyyy") ?? "",
                PrintCount = order.PrintCount.ToString(),
            };

            return dto;
        }

        private async Task<List<GetCurrentOrderItemsDTO>> GetOrderItems(long orderId)
        {
            var items = await _dataContext.Order
                .Include(o => o.Items)
                .Include(c => c.Coupon)
                .Where(o => o.Id == orderId)
                .SelectMany(o => o.Items)
                .Where(i => !i.IsVoid)
                .Include(i => i.Menu)
                .Include(i => i.Drink)
                .Include(i => i.AddOn)
                .Include(i => i.Order)
                .Include(i => i.Meal)
                .ToListAsync();

            // Group items by EntryId.
            // For items with no EntryId (child meals), use the parent's EntryId from the Meal property.
            var groupedItems = items
                .GroupBy(i => i.EntryId ?? i.Meal?.EntryId)
                .OrderBy(g => g.Min(i => i.createdAt))
                .Select(g =>
                {
                    // Compute the promo discount amount from the parent order.
                    var promoDiscount = g.Select(i => (i.Order?.DiscountType == DiscountTypeEnum.Promo.ToString()
                                                        ? i.Order?.DiscountAmount ?? 0m
                                                        : 0m))
                                         .FirstOrDefault();
                    // Check for other discount types.
                    var otherDiscount = g.Any(i => i.IsPwdDiscounted || i.IsSeniorDiscounted);

                    // Set HasDiscount to true if there's any other discount or promo discount value is greater than zero.
                    var hasDiscount = otherDiscount || (promoDiscount > 0m);

                    // Build the DTO from the group
                    var dto = new GetCurrentOrderItemsDTO
                    {
                        // Use the group's key or 0 if still null.
                        EntryId = g.Key ?? "",
                        HasDiscount = hasDiscount,
                        PromoDiscountAmount = promoDiscount,
                        IsPwdDiscounted = g.Any(i => i.IsPwdDiscounted),
                        IsSeniorDiscounted = g.Any(i => i.IsSeniorDiscounted),
                        // Order each group so that the parent (Meal == null) comes first.
                        SubOrders = g.OrderBy(i => i.Meal == null ? 0 : 1)
                                     .Select(i => new CurrentOrderItemsSubOrder
                                     {
                                         MenuId = i.Menu?.Id,
                                         DrinkId = i.Drink?.Id,
                                         AddOnId = i.AddOn?.Id,
                                         // Fallback: use Menu name first, then Drink, then AddOn.
                                         Name = i.Menu?.MenuName ?? i.Drink?.MenuName ?? i.AddOn?.MenuName ?? "Unknown",
                                         Size = i.Menu?.Size ?? i.Drink?.Size ?? i.AddOn?.Size,
                                         ItemPrice = i.ItemPrice ?? 0m,
                                         Quantity = i.ItemQTY ?? 1,
                                         IsFirstItem = i.Meal == null
                                     })
                                     .ToList()
                    };

                    // If discount applies, add an extra suborder for discount details.
                    if (dto.HasDiscount && dto.PromoDiscountAmount <= 0)
                    {
                        // Calculate discount based on the current total of suborders.
                        // (Be aware that if you add the discount as a suborder, it might affect TotalPrice.)
                        var discountAmount = dto.SubOrders.Sum(s => s.ItemSubTotal) >= 250
                        ? 250
                        : dto.SubOrders.Sum(s => s.ItemSubTotal) * 0.20m;

                        // Use the first item in the group to determine discount type.
                        var discountName = g.Any(i => i.IsPwdDiscounted) ? "PWD" : "Senior";

                        dto.SubOrders.Add(new CurrentOrderItemsSubOrder
                        {
                            Name = discountName,          // This can be adjusted to show a more descriptive name.
                            ItemPrice = discountAmount, // The discount amount.
                            Quantity = 1,
                            // You can set Size to null or leave it empty.
                            IsFirstItem = false         // Typically discount line is not the first item.
                        });
                    }

                    return dto;
                })
                .ToList();

            var ordersWithCoupons = await _dataContext.Order
                .Include(o => o.Coupon)
                .ThenInclude(c => c.CouponMenus)
                .Where(o => o.Id == orderId)
                .ToListAsync();

            var couponItems = ordersWithCoupons
                .SelectMany(o => o.Coupon)
                .Where(c => c != null)
                .DistinctBy(c => c.CouponCode) // using DistinctBy from System.Linq if available
                .Select(c => new GetCurrentOrderItemsDTO
                {
                    EntryId = $"Coupon-{c.CouponCode}",
                    HasDiscount = false,
                    PromoDiscountAmount = 0m,
                    IsPwdDiscounted = false,
                    IsSeniorDiscounted = false,
                    CouponCode = c.CouponCode,
                    SubOrders = new List<CurrentOrderItemsSubOrder>
                    {
                        new CurrentOrderItemsSubOrder
                        {
                            Name = $"Coupon: {c.CouponCode}",
                            ItemPrice = c.PromoAmount ?? 0m,
                            Quantity = c.CouponItemQuantity ?? 0,
                            IsFirstItem = true
                        }
                    }
                    .Concat(
                        c.CouponMenus?.Where(m => m.MenuIsAvailable)
                        .Select(m => new CurrentOrderItemsSubOrder
                        {
                            MenuId = m.Id,
                            Name = m.MenuName,
                            Size = m.Size,
                            ItemPrice = 0m,
                            Quantity = 1,
                            IsFirstItem = false
                        }) ?? Enumerable.Empty<CurrentOrderItemsSubOrder>()
                    ).ToList()
                })
                .ToList();



            // ✅ Merge regular orders with coupon orders.
            groupedItems.AddRange(couponItems);

            return groupedItems;
        }

        public async Task<XInvoiceReportDTO> XInvoiceReport()
        {
            var pesoCulture = new CultureInfo("en-PH");
            var defaultDecimal = 0m;
            var defaultDate = DateTime.UtcNow;

            bool isTrainMode = await _dataContext.PosTerminalInfo
                .Select(o => o.IsTrainMode)
                .FirstOrDefaultAsync();

            // Safely fetch data with null checks
            var orders = await _dataContext.Order
                .Include(o => o.Cashier)
                .Include(o => o.Items)
                .Include(o => o.AlternativePayments)
                    .ThenInclude(ap => ap.SaleType)
                .Where(o => !o.IsRead && o.IsTrainMode == isTrainMode)
                .ToListAsync() ?? new List<Order>();

            var posInfo = await _dataContext.PosTerminalInfo.FirstOrDefaultAsync();
            if (posInfo == null)
            {
                throw new InvalidOperationException("POS terminal information not configured");
            }

            var ts = await _dataContext.Timestamp
                .Include(t => t.Cashier)
                .Include(t => t.ManagerLog)
                .OrderBy(t => t.Id)
                .LastOrDefaultAsync(o => o.IsTrainMode == isTrainMode);

            // Handle empty orders scenario
            var firstOrder = orders.FirstOrDefault();
            var lastOrder = orders.LastOrDefault();
            var orderCount = orders.Count().ToString();

            // Calculate financials with null protection
            decimal openingFundDec = ts?.CashInDrawerAmount ?? defaultDecimal;

            // Move withdrawal calculation to memory
            var withdrawals = await _dataContext.UserLog
                .Where(mw => mw.Timestamp == ts)
                .ToListAsync();

            decimal withdrawnAmount = withdrawals.Sum(mw => mw.WithdrawAmount);

            // Calculate void and refund amounts in memory
            decimal voidDec = orders.Where(o => o.IsCancelled)
                                  .Sum(o => o?.TotalAmount ?? defaultDecimal);
            string voidCount = orders.Count(o => o.IsCancelled).ToString();

            decimal refundDec = orders.Where(o => o.IsReturned)
                                    .Sum(o => o?.TotalAmount ?? defaultDecimal);
            string refundCount = orders.Count(o => o.IsReturned).ToString();

            // Calculate valid orders total in memory
            decimal validOrdersTotal = orders.Where(o => !o.IsCancelled && !o.IsReturned)
                                           .Sum(o => (o?.CashTendered ?? defaultDecimal) - (o?.ChangeAmount ?? defaultDecimal));

            decimal actualCash = openingFundDec + validOrdersTotal;
            decimal expectedCash = (ts?.CashOutDrawerAmount ?? defaultDecimal) + withdrawnAmount;
            decimal shortOverDec = expectedCash - actualCash;



            // Safe payment processing - moved to memory
            var payments = new Payments
            {
                Cash = orders.Sum(o => (o?.CashTendered ?? defaultDecimal) - (o?.ChangeAmount ?? defaultDecimal)),
                OtherPayments = orders
                    .SelectMany(o => o.AlternativePayments ?? new List<AlternativePayments>())
                    .GroupBy(ap => ap.SaleType?.Name ?? "Unknown")
                    .Select(g => new PaymentDetail
                    {
                        Name = g.Key + $"({g.Count()}) :",
                        Amount = g.Sum(x => x?.Amount ?? defaultDecimal),
                    }).ToList()
            };

            var summary = new TransactionSummary
            {
                CashInDrawer = (ts?.CashOutDrawerAmount ?? defaultDecimal).ToString("C", pesoCulture),
                //CashInDrawer = (((ts?.CashOutDrawerAmount ?? defaultDecimal) - openingFundDec) - (payments.Cash - withdrawnAmount))
                //              .ToString("C", pesoCulture),
                OtherPayments = payments.OtherPayments
            };

            // Build DTO with safe values
            var dto = new XInvoiceReportDTO
            {
                BusinessName = posInfo.RegisteredName ?? "N/A",
                OperatorName = posInfo.OperatedBy ?? "N/A",
                AddressLine = posInfo.Address ?? "N/A",
                VatRegTin = posInfo.VatTinNumber ?? "N/A",
                Min = posInfo.MinNumber ?? "N/A",
                SerialNumber = posInfo.PosSerialNumber ?? "N/A",

                ReportDate = DateTime.Now.ToString("MMMM dd, yyyy", pesoCulture),
                ReportTime = DateTime.Now.ToString("hh:mm tt", pesoCulture),
                StartDateTime = firstOrder?.CreatedAt.LocalDateTime.ToString("MM/dd/yy hh:mm tt", pesoCulture)
                              ?? defaultDate.ToString("MM/dd/yy hh:mm tt", pesoCulture),
                EndDateTime = lastOrder?.CreatedAt.LocalDateTime.ToString("MM/dd/yy hh:mm tt", pesoCulture)
                             ?? defaultDate.ToString("MM/dd/yy hh:mm tt", pesoCulture),

                Cashier = ts?.Cashier != null
                        ? $"{ts.Cashier.UserFName} {ts.Cashier.UserLName}"
                        : "N/A",
                BeginningOrNumber = firstOrder?.InvoiceNumber.ToString("D12") ?? "N/A",
                EndingOrNumber = lastOrder?.InvoiceNumber.ToString("D12") ?? "N/A",
                TransactCount = orderCount,

                OpeningFund = openingFundDec.ToString("C", pesoCulture),
                VoidAmount = voidDec.ToString("C", pesoCulture),
                VoidCount = voidCount,
                Refund = refundDec.ToString("C", pesoCulture),
                RefundCount = refundCount,
                Withdrawal = withdrawnAmount.ToString("C", pesoCulture),

                Payments = payments,
                TransactionSummary = summary,
                ShortOver = shortOverDec.ToString("C", pesoCulture)
            };

            // Mark orders as read if any exist
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    order.IsRead = true;
                }
                await _dataContext.SaveChangesAsync();
            }

            return dto;
        }

        public async Task<ZInvoiceReportDTO> ZInvoiceReport()
        {
            var pesoCulture = new CultureInfo("en-PH");
            var defaultDecimal = 0m;
            var today = DateTime.Today;
            var defaultDate = today;

            bool isTrainMode = await _dataContext.PosTerminalInfo
                .Select(o => o.IsTrainMode)
                .FirstOrDefaultAsync();

            var orders = await _dataContext.Order
                .Where(o => o.IsTrainMode == isTrainMode)
                .Include(o => o.Items)
                .Include(o => o.AlternativePayments)
                    .ThenInclude(ap => ap.SaleType)
                .ToListAsync() ?? new List<Order>();

            // Initialize empty collections to prevent null references
            var allTimestamps = await _dataContext.Timestamp
                .Where(t => t.IsTrainMode == isTrainMode)
                .Include(t => t.Cashier)
                .Include(t => t.ManagerLog)
                .ToListAsync() ?? new List<Timestamp>();

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

            // Handle empty scenario for dates
            var startDate = orders.Any() ? orders.Min(t => t.CreatedAt.LocalDateTime) : DateTime.Now;
            var endDate = orders.Any() ? orders.Max(t => t.CreatedAt.LocalDateTime) : DateTime.Now;

            // Withdrawal Amount
            var withdrawnAmount = allTimestamps
                .Where(t => t.TsOut.HasValue
                    && t.TsOut.Value.Date == today)
                .SelectMany(t => t.ManagerLog)
                .Where(mw => mw?.Action == "Cash Withdrawal")
                .Sum(mw => mw?.WithdrawAmount ?? defaultDecimal);

            var allRegularOrders = orders.Where(o => !o.IsCancelled && !o.IsReturned).ToList();
            var allVoidOrders = orders.Where(o => o.IsCancelled).ToList();
            var allReturnOrders = orders.Where(o => o.IsReturned).ToList();

            // BREAKDOWN OF SALES
            var regularOrders = allRegularOrders.Where(o => o.CreatedAt.Date == today).ToList();
            var voidOrders = allVoidOrders.Where(o => o.CreatedAt.Date == today).ToList();
            var returnOrders = allReturnOrders.Where(o => o.CreatedAt.Date == today).ToList();

            // Accumulated Sales
            decimal salesForTheDay = regularOrders.Where(c => c.CreatedAt.Date == today).Sum(o => o.TotalAmount - o.DiscountAmount ?? defaultDecimal);
            decimal previousAccumulatedSales = allRegularOrders.Where(c => c.CreatedAt.Date < today).Sum(o => o.TotalAmount - o.DiscountAmount ?? defaultDecimal);
            decimal presentAccumulatedSales = previousAccumulatedSales + salesForTheDay;

            // Financial calculations with default values
            decimal grossSales = regularOrders.Sum(o => o?.TotalAmount ?? defaultDecimal);
            decimal totalVoid = voidOrders.Sum(o => o?.TotalAmount ?? defaultDecimal);
            decimal totalReturns = returnOrders.Sum(o => o?.TotalAmount ?? defaultDecimal);
            decimal totalDiscounts = regularOrders.Sum(o => o?.DiscountAmount ?? defaultDecimal);
            decimal cashSales = regularOrders.Sum(o => o?.CashTendered - o?.ChangeAmount ?? defaultDecimal);

            //decimal cashLessSales = regularOrders.Sum(o => o?.AlternativePayments.Sum(c=>c.Amount) ?? defaultDecimal);

            decimal netAmount = grossSales - totalReturns - totalVoid - totalDiscounts;

            // VAT calculations with defaults
            decimal vatableSales = regularOrders.Sum(v => v?.VatSales ?? defaultDecimal);
            decimal vatAmount = regularOrders.Sum(o => o?.VatAmount ?? defaultDecimal);
            decimal vatExempt = regularOrders.Sum(o => o?.VatExempt ?? defaultDecimal);
            decimal zeroRated = 0m;

            // Cash in Drawer
            decimal cashInDrawer = allTimestamps
                .Where(t => t.TsOut.HasValue
                    && t.TsOut.Value.Date == today)
                .Sum(s => s.CashOutDrawerAmount) ?? defaultDecimal;

            // Opening Fund
            decimal openingFund = allTimestamps
                .Where(t => t.TsIn.Value.Date == today)
                .Sum(s => s.CashInDrawerAmount) ?? defaultDecimal;

            decimal actualCash = openingFund + cashSales;
            decimal expectedCash = cashInDrawer + withdrawnAmount;
            decimal shortOver = expectedCash - actualCash;


            var knownDiscountTypes = Enum.GetNames(typeof(DiscountTypeEnum)).ToList();

            decimal seniorDiscount = regularOrders
                .Where(s => s.DiscountType == DiscountTypeEnum.Senior.ToString())
                .Sum(s => s.DiscountAmount) ?? 0m;

            string seniorCount = regularOrders
                .Where(s => s.DiscountType == DiscountTypeEnum.Senior.ToString())
                .Count()
                .ToString();

            decimal pwdDiscount = regularOrders
                .Where(s => s.DiscountType == DiscountTypeEnum.Pwd.ToString())
                .Sum(s => s.DiscountAmount) ?? 0m;

            string pwdCount = regularOrders
                .Where(s => s.DiscountType == DiscountTypeEnum.Pwd.ToString())
                .Count()
                .ToString();

            decimal otherDiscount = regularOrders
                .Where(s => !knownDiscountTypes.Contains(s.DiscountType))
                .Sum(s => s.DiscountAmount) ?? 0m;

            string otherCount = regularOrders
                .Where(s => !knownDiscountTypes.Contains(s.DiscountType))
                .Count()
                .ToString();

            // Safe payment processing
            var payments = new Payments
            {
                Cash = cashSales,
                OtherPayments = orders
                .SelectMany(o => o.AlternativePayments != null && o.CreatedAt.Date == today ? o.AlternativePayments : new List<AlternativePayments>())
                .GroupBy(ap => ap.SaleType?.Name ?? "Unknown")
                .Select(g => new PaymentDetail
                {
                    Name = g.Key + $"({g.Count()}):",
                    Amount = g.Sum(x => x.Amount),
                }).ToList()
            };

            // Build DTO with zero defaults
            var dto = new ZInvoiceReportDTO
            {
                BusinessName = posInfo.RegisteredName ?? "N/A",
                OperatorName = posInfo.OperatedBy ?? "N/A",
                AddressLine = posInfo.Address ?? "N/A",
                VatRegTin = posInfo.VatTinNumber ?? "N/A",
                Min = posInfo.MinNumber ?? "N/A",
                SerialNumber = posInfo.PosSerialNumber ?? "N/A",

                ReportDate = DateTime.Now.ToString("MMMM dd, yyyy", pesoCulture),
                ReportTime = DateTime.Now.ToString("hh:mm tt", pesoCulture),
                StartDateTime = startDate.ToString("MM/dd/yy hh:mm tt", pesoCulture),
                EndDateTime = endDate.ToString("MM/dd/yy hh:mm tt", pesoCulture),

                // Order numbers
                BeginningSI = GetOrderNumber(allRegularOrders.Min(o => o?.InvoiceNumber)),
                EndingSI = GetOrderNumber(allRegularOrders.Max(o => o?.InvoiceNumber)),
                BeginningVoid = GetOrderNumber(allVoidOrders.Min(o => o?.InvoiceNumber)),
                EndingVoid = GetOrderNumber(allVoidOrders.Max(o => o?.InvoiceNumber)),
                BeginningReturn = GetOrderNumber(allReturnOrders.Min(o => o?.InvoiceNumber)),
                EndingReturn = GetOrderNumber(allReturnOrders.Max(o => o?.InvoiceNumber)),
                TransactCount = orders.Count().ToString(),

                // Always zero when empty
                ResetCounter = isTrainMode ? posInfo.ResetCounterTrainNo.ToString() : posInfo.ResetCounterNo.ToString(),
                ZCounter = isTrainMode ? posInfo.ZCounterTrainNo.ToString() : posInfo.ZCounterNo.ToString(),

                // Financial summaries
                PresentAccumulatedSales = presentAccumulatedSales.ToString("C", pesoCulture),
                PreviousAccumulatedSales = previousAccumulatedSales.ToString("C", pesoCulture),
                SalesForTheDay = salesForTheDay.ToString("C", pesoCulture),

                SalesBreakdown = new SalesBreakdown
                {
                    VatableSales = vatableSales.ToString("C", pesoCulture),
                    VatAmount = vatAmount.ToString("C", pesoCulture),
                    VatExemptSales = vatExempt.ToString("C", pesoCulture),
                    ZeroRatedSales = zeroRated.ToString("C", pesoCulture),
                    GrossAmount = grossSales.ToString("C", pesoCulture),
                    LessDiscount = totalDiscounts.ToString("C", pesoCulture),
                    LessReturn = totalReturns.ToString("C", pesoCulture),
                    LessVoid = totalVoid.ToString("C", pesoCulture),
                    LessVatAdjustment = defaultDecimal.ToString("C", pesoCulture),
                    NetAmount = netAmount.ToString("C", pesoCulture)
                },

                TransactionSummary = new TransactionSummary
                {
                    CashInDrawer = cashInDrawer.ToString("C", pesoCulture),
                    OtherPayments = payments.OtherPayments
                },

                DiscountSummary = new DiscountSummary
                {
                    SeniorCitizen = seniorDiscount.ToString("C", pesoCulture),
                    SeniorCitizenCount = seniorCount,
                    PWD = pwdDiscount.ToString("C", pesoCulture),
                    PWDCount = pwdCount,
                    Other = otherDiscount.ToString("C", pesoCulture),
                    OtherCount = otherCount
                },

                SalesAdjustment = new SalesAdjustment
                {
                    Return = totalReturns.ToString("C", pesoCulture),
                    ReturnCount = returnOrders.Count().ToString(),
                    Void = totalVoid.ToString("C", pesoCulture),
                    VoidCount = voidOrders.Count().ToString(),
                },

                VatAdjustment = new VatAdjustment
                {
                    SCTrans = defaultDecimal.ToString("C", pesoCulture),
                    PWDTrans = defaultDecimal.ToString("C", pesoCulture),
                    RegDiscTrans = defaultDecimal.ToString("C", pesoCulture),
                    ZeroRatedTrans = defaultDecimal.ToString("C", pesoCulture),
                    VatOnReturn = defaultDecimal.ToString("C", pesoCulture),
                    OtherAdjustments = defaultDecimal.ToString("C", pesoCulture)
                },

                OpeningFund = openingFund.ToString("C", pesoCulture),
                Withdrawal = withdrawnAmount.ToString("C", pesoCulture),
                PaymentsReceived = (cashSales + payments.OtherPayments.Sum(s => s.Amount)).ToString("C", pesoCulture),
                ShortOver = shortOver.ToString("C", pesoCulture)
            };

            if (isTrainMode)
            {
                posInfo.ZCounterTrainNo += 1;
            }
            else
            {


                posInfo.ZCounterNo += 1;
            }
            await _dataContext.SaveChangesAsync();

            return dto;
        }

        private string GetOrderNumber(long? orderId)
        {
            return orderId.HasValue ? orderId.Value.ToString("D12") : 0.ToString("D12");
        }

        public async Task<List<UserActionLogDTO>> UserActionLog(bool isManagerLog, DateTime fromDate, DateTime toDate)
        {
            var logs = new List<UserActionLogDTO>();
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1);

            // Common query parts
            var userLogsQuery = _dataContext.UserLog
                .Include(m => m.Cashier)
                .Include(m => m.Manager)
                .Where(c =>
                    ((isManagerLog && c.Manager != null) || (!isManagerLog && c.Cashier != null)) &&
                    c.CreatedAt >= start &&
                    c.CreatedAt < end);

            var userLogs = await userLogsQuery
                .Select(m => new UserActionLogDTO
                {
                    Name = m.Manager.UserFName + " " + m.Manager.UserLName,
                    CashierName = m.Cashier.UserFName + " " + m.Cashier.UserLName,
                    Action = m.Action,
                    ManagerEmail = m.Manager.UserEmail,
                    CashierEmail = m.Cashier.UserEmail,
                    Amount = m.WithdrawAmount > 0
                        ? string.Format(CultureInfo.InvariantCulture, "₱{0:N2}", m.WithdrawAmount)
                        : null,
                    ActionDate = m.CreatedAt.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
                    SortActionDate = m.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            logs.AddRange(userLogs);

            // Process timestamps - Modified query for SQLite compatibility
            var timestamps = await _dataContext.Timestamp
                .AsNoTracking()
                .Include(t => t.Cashier)
                .Include(t => t.ManagerIn)
                .Include(t => t.ManagerOut)
                .ToListAsync();

            // Filter timestamps in memory after fetching
            var filteredTimestamps = timestamps.Where(t =>
                (t.TsIn.HasValue && t.TsIn.Value.DateTime.Date >= start && t.TsIn.Value.DateTime.Date < end) ||
                (t.TsOut.HasValue && t.TsOut.Value.DateTime.Date >= start && t.TsOut.Value.DateTime.Date < end))
                .ToList();

            ProcessTimestamps(filteredTimestamps, logs);

            return logs.OrderBy(l => l.SortActionDate).ToList();
        }
        private void ProcessTimestamps(List<Timestamp> timestamps, List<UserActionLogDTO> logs)
        {
            foreach (var t in timestamps)
            {
                var cashierName = t.Cashier != null
                    ? $"{t.Cashier.UserFName} {t.Cashier.UserLName}"
                    : "—";
                var cashierEmail = t.Cashier?.UserEmail ?? "—";

                // Login (TsIn)
                if (t.TsIn.HasValue)
                {
                    var tsIn = t.TsIn.Value;
                    if (t.CashInDrawerAmount.HasValue)
                    {
                        AddTimestampLog(logs, t.ManagerIn, tsIn,
                            "Set Cash in Drawer", t.CashInDrawerAmount, cashierName, cashierEmail);
                    }
                    else
                    {
                        AddTimestampLog(logs, t.ManagerIn, tsIn,
                            "Log In", null, cashierName, cashierEmail);
                    }
                }

                // Logout and/or Cash Out (TsOut)
                if (t.TsOut.HasValue)
                {
                    var tsOut = t.TsOut.Value;
                    var mgr = t.ManagerOut ?? t.ManagerIn;

                    if (t.CashOutDrawerAmount.HasValue)
                    {
                        AddTimestampLog(logs, mgr, tsOut,
                            "Set Cash out Drawer", t.CashOutDrawerAmount, cashierName, cashierEmail);
                    }
                    else
                    {
                        AddTimestampLog(logs, mgr, tsOut,
                            "Log Out", null, cashierName, cashierEmail);
                    }
                }
            }
        }


        private void AddTimestampLog(
            List<UserActionLogDTO> logs,
            User? manager,
            DateTimeOffset timestamp,
            string actionType,
            decimal? amount,
            string? cashierName,
            string? cashierEmail)
        {
            logs.Add(new UserActionLogDTO
            {
                Name = $"{manager.UserFName} {manager.UserLName}",
                Action = actionType.ToString(),
                ManagerEmail = manager.UserEmail,
                CashierName = cashierName,
                CashierEmail = cashierEmail,
                Amount = amount.HasValue
                    ? string.Format(CultureInfo.InvariantCulture, "₱{0:N2}", amount.Value)
                    : null,
                ActionDate = timestamp.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
                SortActionDate = timestamp.ToLocalTime().DateTime
            });
        }
    }
}
