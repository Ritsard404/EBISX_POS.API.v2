﻿using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Models.Journal;
using EBISX_POS.API.Services.DTO.Journal;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EBISX_POS.API.Services.Repositories
{
    public class JournalRepository(JournalContext _journal, DataContext _dataContext, ILogger<JournalRepository> _logger) : IJournal
    {
        public async Task<List<AccountJournal>> AccountJournals()
        {
            return await _journal.AccountJournal.ToListAsync();
        }

        //public async Task<(bool isSuccess, string message)> AddItemsJournal(long orderId)
        //{

        //    var items = await _dataContext.Order
        //        .Include(o => o.Items)
        //        .Include(c => c.Coupon)
        //        .Where(o => o.Id == orderId)
        //        .SelectMany(o => o.Items)
        //        .Include(i => i.Menu)
        //        .Include(i => i.Drink)
        //        .Include(i => i.AddOn)
        //        .Include(i => i.Order)
        //        .Include(i => i.Meal)
        //        .ToListAsync();

        //    if (items.IsNullOrEmpty())
        //        return (false, "Order not found");

        //    var journals = new List<AccountJournal>();

        //    foreach (var item in items)
        //    {
        //        var journal = new AccountJournal
        //        {
        //            EntryNo = item.Order.Id,
        //            EntryLineNo = 3, // Adjust as 
        //            Status = item.IsVoid ? "Unposted" : "Posted",
        //            EntryName = item.EntryId,
        //            AccountName = item.Menu?.MenuName ?? item.Drink?.MenuName ?? item.AddOn?.MenuName ?? "Unknown",
        //            EntryDate = item.createdAt.DateTime,
        //            Description = item.Menu?.MenuName != null ? "Menu"
        //            : item.Drink?.MenuName != null ? "Drink"
        //            : "Add-On",
        //            QtyOut = item.ItemQTY,
        //            Price = Convert.ToDouble(item.ItemPrice),

        //            // Optionally, set other properties as needed.
        //        };
        //    }


        //    return (true, "Success");
        //}
        public async Task<(bool isSuccess, string message)> AddItemsJournal(long orderId)
        {
            try
            {
                _logger.LogInformation("Starting AddItemsJournal for OrderId: {OrderId}", orderId);

                var order = await _dataContext.Order
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Menu)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Drink)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.AddOn)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Meal)
                    .Include(o => o.Coupon)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                    return (false, "Order not found");
                }

                // Skip journal entries for training mode
                if (order.IsTrainMode)
                {
                    _logger.LogInformation("Skipping journal entries for training mode order {OrderId}", orderId);
                    return (true, "Training mode order - no journal entries created");
                }

                if (order.Items == null || !order.Items.Any())
                {
                    _logger.LogWarning("Order {OrderId} has no items.", orderId);
                    return (false, "No items found in the order");
                }

                var journals = new List<AccountJournal>();

                foreach (var item in order.Items)
                {
                    var accountName = item.Menu?.MenuName ?? item.Drink?.MenuName ?? item.AddOn?.MenuName ?? "Unknown";
                    var description = item.Menu != null ? "Menu"
                                     : item.Drink != null ? "Drink"
                                     : item.AddOn != null ? "Add-On"
                                     : "Unknown";

                    var journal = new AccountJournal
                    {
                        EntryNo = order.InvoiceNumber,
                        EntryLineNo = 3, // Adjust if needed
                        Status = item.IsVoid ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                        EntryName = item.EntryId ?? "",
                        AccountName = accountName,
                        EntryDate = item.createdAt.DateTime,
                        Description = description,
                        QtyOut = item.ItemQTY,
                        Price = Convert.ToDouble(item.ItemPrice)
                    };

                    journals.Add(journal);

                    _logger.LogInformation("Prepared journal entry: AccountName={AccountName}, Description={Description}, QtyOut={Qty}, Price={Price}, EntryNo={EntryNo}",
                        journal.AccountName, journal.Description, journal.QtyOut, journal.Price, journal.EntryNo);
                }

                if (!journals.Any())
                {
                    _logger.LogWarning("No valid journal entries generated for Order {OrderId}", orderId);
                    return (false, "No valid journal entries to add.");
                }

                _journal.AccountJournal.AddRange(journals);
                await _journal.SaveChangesAsync();

                _logger.LogInformation("Successfully added {Count} journal entries for Order {OrderId}.", journals.Count, orderId);
                return (true, "Journal entries successfully added.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding journal entries for Order {OrderId}.", orderId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool isSuccess, string message)> AddPwdScAccountJournal(AddPwdScAccountJournalDTO journalDTO)
        {
            if (journalDTO is null)
            {
                return (false, "Input cannot be null.");
            }

            if (journalDTO.PwdScInfo == null || !journalDTO.PwdScInfo.Any())
            {
                return (false, "PwdScInfo list cannot be empty.");
            }

            try
            {
                // Get the order to check training mode and invoice number
                var order = await _dataContext.Order
                    .FirstOrDefaultAsync(o => o.Id == journalDTO.OrderId);

                if (order == null)
                {
                    return (false, "Order not found.");
                }

                // Skip journal entries for training mode
                if (order.IsTrainMode)
                {
                    _logger.LogInformation("Skipping PWD/SC journal entries for training mode order {OrderId}", order.Id);
                    return (true, "Training mode order - no journal entries created");
                }

                // Prepare a list of valid journal entries
                var journals = new List<AccountJournal>();

                foreach (var pwdOrSc in journalDTO.PwdScInfo)
                {
                    if (string.IsNullOrWhiteSpace(pwdOrSc.Name))
                    {
                        _logger.LogError("Invalid journal entry: AccountName is null or empty. Skipping entry with Reference {Reference}.", pwdOrSc.OscaNum);
                        continue;
                    }

                    var journal = new AccountJournal
                    {
                        EntryNo = order.InvoiceNumber,
                        EntryLineNo = 5,
                        Status = journalDTO.Status ?? "Posted",
                        AccountName = pwdOrSc.Name,
                        Reference = pwdOrSc.OscaNum,
                        EntryDate = journalDTO.EntryDate,
                        EntryName = journalDTO.IsPWD ? "PWD" : "Senior"
                    };

                    journals.Add(journal);

                    _logger.LogInformation("Prepared AccountJournal entry: AccountName={AccountName}, Reference={Reference}, EntryDate={EntryDate}, EntryNo={EntryNo}",
                        journal.AccountName, journal.Reference, journal.EntryDate, journal.EntryNo);
                }

                if (!journals.Any())
                {
                    return (false, "No valid journal entries to add. Please check your input.");
                }

                await _journal.AccountJournal.AddRangeAsync(journals);
                await _journal.SaveChangesAsync();

                return (true, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding account journal entries.");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool isSuccess, string message)> AddPwdScJournal(long orderId)
        {
            _logger.LogInformation("Starting AddPwdScJournal for OrderId: {OrderId}", orderId);

            if (orderId <= 0)
            {
                _logger.LogWarning("Invalid OrderId: {OrderId}", orderId);
                return (false, "Invalid order ID.");
            }

            // 1) Load the order so we can read the PWD/SC fields
            var order = await _dataContext.Order
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                return (false, "Order not found.");
            }

            // Skip journal entries for training mode
            if (order.IsTrainMode)
            {
                _logger.LogInformation("Skipping PWD/SC journal entries for training mode order {OrderId}", orderId);
                return (true, "Training mode order - no journal entries created");
            }

            // 2) Guard: need both names and OSCAs
            if (string.IsNullOrWhiteSpace(order.EligibleDiscNames) ||
                string.IsNullOrWhiteSpace(order.OSCAIdsNum))
            {
                _logger.LogWarning("No PWD/SC data on Order {OrderId}. Names: '{Names}', OSCAs: '{Oscas}'",
                    orderId, order.EligibleDiscNames, order.OSCAIdsNum);
                return (false, "No PWD/SC information to journal.");
            }

            // 3) Split into lists
            var names = order.EligibleDiscNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .ToList();

            var oscas = order.OSCAIdsNum
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .ToList();

            // 4) Pair them up to the smaller count
            var count = Math.Min(names.Count, oscas.Count);
            if (count == 0)
            {
                _logger.LogWarning("After splitting, no valid PWD/SC pairs for Order {OrderId}.", orderId);
                return (false, "No valid PWD/SC pairs found.");
            }

            // 5) Build journal entries
            var journals = new List<AccountJournal>();
            int lineNo = 5;  // starting line number for PWD/SC entries

            for (int i = 0; i < count; i++)
            {
                var name = names[i];
                var osca = oscas[i];

                var journal = new AccountJournal
                {
                    EntryNo = order.InvoiceNumber,
                    EntryLineNo = lineNo++,
                    Status = order.IsCancelled ? "Unposted" : "Posted",
                    AccountName = name,
                    Reference = osca,
                    EntryName = order.DiscountType ?? "",
                    EntryDate = order.CreatedAt.DateTime
                };

                journals.Add(journal);

                _logger.LogInformation(
                    "Prepared PWD/SC journal #{LineNo}: Name={Name}, OSCA={Osca}, EntryNo={EntryNo}",
                    journal.EntryLineNo, name, osca, journal.EntryNo);
            }

            // 6) Persist
            try
            {
                _journal.AccountJournal.AddRange(journals);
                await _journal.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully added {Count} PWD/SC journal entries for Order {OrderId}.",
                    journals.Count, orderId);

                return (true, $"{journals.Count} PWD/SC entries added.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error saving PWD/SC journal entries for Order {OrderId}.",
                    orderId);

                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool isSuccess, string message)> AddTendersJournal(long orderId)
        {
            _logger.LogInformation("Starting AddTendersJournal for OrderId: {OrderId}", orderId);

            if (orderId <= 0)
            {
                _logger.LogWarning("Invalid OrderId: {OrderId}", orderId);
                return (false, "Invalid order ID.");
            }

            // Load the order plus any AlternativePayments
            var order = await _dataContext.Order
                .Include(o => o.AlternativePayments)
                    .ThenInclude(t => t.SaleType)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                return (false, "Order not found");
            }

            // Skip journal entries for training mode
            if (order.IsTrainMode)
            {
                _logger.LogInformation("Skipping tenders journal entries for training mode order {OrderId}", orderId);
                return (true, "Training mode order - no journal entries created");
            }

            var journals = new List<AccountJournal>();

            // 1) Record the cash tendered on the order itself
            if (order.CashTendered > 0)
            {
                journals.Add(new AccountJournal
                {
                    EntryNo = order.InvoiceNumber,
                    EntryLineNo = 0,
                    Status = order.IsCancelled ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                    EntryName = "Cash Tendered",
                    AccountName = "Cash",
                    Description = "Cash Tendered",
                    Debit = order.IsReturned ? 0 : Convert.ToDouble(order.CashTendered),
                    Credit = order.IsReturned ? Convert.ToDouble(order.CashTendered) : 0,
                    EntryDate = order.CreatedAt.DateTime
                });
            }

            // 2) Record any alternative payments (card, gift-card, etc.)
            if (order.AlternativePayments != null)
            {
                foreach (var tender in order.AlternativePayments)
                {
                    var journal = new AccountJournal
                    {
                        EntryNo = order.InvoiceNumber,
                        EntryLineNo = 0,
                        Status = order.IsCancelled ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                        EntryName = tender.SaleType.Name,
                        AccountName = tender.SaleType.Account,
                        Description = tender.SaleType.Type,
                        Reference = tender.Reference,
                        Debit = order.IsReturned ? 0 : Convert.ToDouble(tender.Amount),
                        Credit = order.IsReturned ? Convert.ToDouble(tender.Amount) : 0,
                        EntryDate = order.CreatedAt.DateTime
                    };

                    journals.Add(journal);

                    _logger.LogInformation("Prepared tender journal #{LineNo}: Account={Account}, Credit={Credit}, EntryNo={EntryNo}",
                        journal.EntryLineNo, journal.AccountName, journal.Credit, journal.EntryNo);
                }
            }

            if (!journals.Any())
            {
                _logger.LogWarning("No payment entries to journal for Order {OrderId}.", orderId);
                return (false, "No payment entries found.");
            }

            try
            {
                _journal.AccountJournal.AddRange(journals);
                await _journal.SaveChangesAsync();

                _logger.LogInformation("Successfully added {Count} payment journal entries for Order {OrderId}.", journals.Count, orderId);
                return (true, $"{journals.Count} payment entries added.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payment journal entries for Order {OrderId}.", orderId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool isSuccess, string message)> AddTotalsJournal(long orderId)
        {
            _logger.LogInformation("Starting AddTotalsJournal for OrderId: {OrderId}", orderId);

            if (orderId <= 0)
            {
                _logger.LogWarning("Invalid OrderId: {OrderId}", orderId);
                return (false, "Invalid order ID.");
            }

            // Load just the Order so we can grab TotalAmount, DiscountType, DiscountAmount
            var order = await _dataContext.Order
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                return (false, "Order not found.");
            }

            // Skip journal entries for training mode
            if (order.IsTrainMode)
            {
                _logger.LogInformation("Skipping totals journal entries for training mode order {OrderId}", orderId);
                return (true, "Training mode order - no journal entries created");
            }

            var journals = new List<AccountJournal>();

            // 1) Discount line (EntryLineNo = 9)
            if (order.DiscountAmount > 0)
            {
                var discountAccount = !string.IsNullOrWhiteSpace(order.DiscountType)
                    ? order.DiscountType
                    : "Discount";

                journals.Add(new AccountJournal
                {
                    EntryNo = order.InvoiceNumber,
                    EntryLineNo = 10,
                    EntryName = "Discount Amount",
                    Status = order.IsCancelled ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                    AccountName = discountAccount,
                    Description = "Discount",
                    Debit = order.IsReturned ? 0 : Convert.ToDouble(order.DiscountAmount),
                    Credit = order.IsReturned ? Convert.ToDouble(order.DiscountAmount) : 0,
                    EntryDate = order.CreatedAt.DateTime
                });

                _logger.LogInformation("Prepared discount journal (Line 9): Account={Account}, Debit={Amt}, EntryNo={EntryNo}",
                    discountAccount, order.DiscountAmount, order.InvoiceNumber);
            }

            // 2) Total line (EntryLineNo = 10)
            journals.Add(new AccountJournal
            {
                EntryNo = order.InvoiceNumber,
                EntryLineNo = 10,
                EntryName = "Total Amount",
                Status = order.IsCancelled ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                AccountName = "Sales",
                Description = "Order Total",
                Debit = order.IsReturned ? 0 : Convert.ToDouble(order.TotalAmount),
                Credit = order.IsReturned ? Convert.ToDouble(order.TotalAmount) : 0,
                EntryDate = order.CreatedAt.DateTime
            });

            journals.Add(new AccountJournal
            {
                EntryNo = order.InvoiceNumber,
                EntryLineNo = 10,
                EntryName = "VAT Amount",
                Status = order.IsCancelled ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                AccountName = "VAT",
                Description = "Order VAT",
                Vatable = Convert.ToDouble(order.VatAmount),
                EntryDate = order.CreatedAt.DateTime
            });

            journals.Add(new AccountJournal
            {
                EntryNo = order.InvoiceNumber,
                EntryLineNo = 10,
                EntryName = "VAT Exempt Amount",
                Status = order.IsCancelled ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                AccountName = "VAT Exempt",
                Description = "Order VAT Exempt",
                Vatable = Convert.ToDouble(order.VatExempt),
                EntryDate = order.CreatedAt.DateTime
            });

            journals.Add(new AccountJournal
            {
                EntryNo = order.InvoiceNumber,
                EntryLineNo = 10,
                EntryName = "Sub Total",
                Status = order.IsCancelled ? "Unposted" : order.IsReturned ? "Returned" : "Posted",
                AccountName = "SubTotal",
                Description = "Order SubTotal",
                SubTotal = Convert.ToDouble(order.DueAmount),
                EntryDate = order.CreatedAt.DateTime
            });

            _logger.LogInformation("Prepared total journal (Line 10): Account=Sales, Credit={Amt}, EntryNo={EntryNo}",
                order.TotalAmount, order.InvoiceNumber);

            if (!journals.Any())
            {
                _logger.LogWarning("No totals entries to journal for Order {OrderId}.", orderId);
                return (false, "No totals to journal.");
            }

            try
            {
                _journal.AccountJournal.AddRange(journals);
                await _journal.SaveChangesAsync();

                _logger.LogInformation("Successfully added {Count} totals journal entries for Order {OrderId}.", journals.Count, orderId);
                return (true, $"{journals.Count} totals entries added.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving totals journal entries for Order {OrderId}.", orderId);
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool isSuccess, string message)> TruncateOrders()
        {
            try
            {
                bool isTrainMode = await _dataContext.PosTerminalInfo
                    .Select(o => o.IsTrainMode)
                    .FirstOrDefaultAsync();

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

                var resetId = isTrainMode ? posInfo.ResetCounterTrainNo : posInfo.ResetCounterNo;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                // Get the source database paths
                var sourceOrderDbPath = _dataContext.Database.GetDbConnection().DataSource;
                var sourceJournalDbPath = _journal.Database.GetDbConnection().DataSource;

                // Create backup database files
                var orderBackupPath = Path.Combine(backupDir, $"Order_Backup_{resetId}_{timestamp}{(isTrainMode ? "_Train" : "")}.db");
                var journalBackupPath = Path.Combine(backupDir, $"AccountJournal_Backup_{resetId}_{timestamp}{(isTrainMode ? "_Train" : "")}.db");

                // Backup Order table
                using (var orderBackupConnection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={orderBackupPath}"))
                {
                    await orderBackupConnection.OpenAsync();
                    using var orderBackupCommand = orderBackupConnection.CreateCommand();
                    
                    // First create the table structure
                    orderBackupCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS [Order] (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            InvoiceNumber INTEGER NOT NULL,
                            OrderType TEXT NOT NULL,
                            TotalAmount REAL NOT NULL,
                            CashTendered REAL,
                            DueAmount REAL,
                            TotalTendered REAL,
                            ChangeAmount REAL,
                            VatSales REAL,
                            VatExempt REAL,
                            VatAmount REAL,
                            CreatedAt TEXT NOT NULL,
                            IsCancelled INTEGER NOT NULL DEFAULT 0,
                            IsReturned INTEGER NOT NULL DEFAULT 0,
                            IsRead INTEGER NOT NULL DEFAULT 0,
                            IsTrainMode INTEGER NOT NULL DEFAULT 0,
                            IsPending INTEGER NOT NULL DEFAULT 1,
                            DiscountType TEXT,
                            DiscountAmount REAL,
                            DiscountPercent INTEGER,
                            EligiblePwdScCount INTEGER,
                            EligibleDiscNames TEXT,
                            OSCAIdsNum TEXT,
                            CashierId INTEGER,
                            EntryId TEXT
                        )";
                    await orderBackupCommand.ExecuteNonQueryAsync();

                    // Then copy the data
                    orderBackupCommand.CommandText = $@"
                        ATTACH DATABASE '{sourceOrderDbPath}' AS source;
                        INSERT INTO [Order] 
                        SELECT * FROM source.[Order];
                        DETACH DATABASE source;";
                    await orderBackupCommand.ExecuteNonQueryAsync();
                }

                // Backup AccountJournal table
                using (var journalBackupConnection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={journalBackupPath}"))
                {
                    await journalBackupConnection.OpenAsync();
                    using var journalBackupCommand = journalBackupConnection.CreateCommand();
                    
                    // First create the table structure
                    journalBackupCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS AccountJournal (
                            unique_id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Entry_Type TEXT NOT NULL DEFAULT '',
                            Entry_No INTEGER,
                            Entry_Line_No INTEGER DEFAULT 0,
                            Entry_Date TEXT NOT NULL DEFAULT '2001-01-01',
                            Entry_Name TEXT NOT NULL DEFAULT '',
                            group_id TEXT NOT NULL DEFAULT '',
                            AccountName TEXT NOT NULL DEFAULT '',
                            Description TEXT NOT NULL DEFAULT '',
                            Reference TEXT NOT NULL DEFAULT '',
                            Branch TEXT NOT NULL DEFAULT '',
                            TerminalNo INTEGER DEFAULT 0,
                            Debit REAL DEFAULT 0,
                            Credit REAL DEFAULT 0,
                            AccountBalance REAL DEFAULT 0,
                            Status TEXT NOT NULL DEFAULT '',
                            cleared TEXT NOT NULL DEFAULT '',
                            clearingref INTEGER DEFAULT 0,
                            costcenter TEXT NOT NULL DEFAULT '',
                            accountno TEXT NOT NULL DEFAULT '',
                            costcenterdesc TEXT NOT NULL DEFAULT '',
                            linetype TEXT NOT NULL DEFAULT '',
                            linetype_transno INTEGER DEFAULT 0,
                            ItemID TEXT NOT NULL DEFAULT '',
                            ItemDesc TEXT,
                            Unit TEXT NOT NULL DEFAULT '',
                            QtyIn REAL DEFAULT 0,
                            QtyOut REAL DEFAULT 0,
                            QtyPerBaseUnit REAL DEFAULT 1,
                            QtyBalanceInBaseUnit REAL DEFAULT 0,
                            Cost REAL DEFAULT 0,
                            Price REAL DEFAULT 0,
                            Discrate REAL DEFAULT 0,
                            Discamt REAL DEFAULT 0,
                            TotalCost REAL DEFAULT 0,
                            TotalPrice REAL DEFAULT 0,
                            received REAL DEFAULT 0,
                            delivered REAL NOT NULL DEFAULT 0,
                            tax_id TEXT NOT NULL DEFAULT '',
                            tax_account TEXT NOT NULL DEFAULT '',
                            tax_type TEXT NOT NULL DEFAULT '',
                            tax_rate REAL DEFAULT 0,
                            tax_total REAL DEFAULT 0,
                            sub_total REAL DEFAULT 0,
                            serial TEXT,
                            chassis TEXT NOT NULL DEFAULT '',
                            engine TEXT NOT NULL DEFAULT '',
                            itemtype TEXT NOT NULL DEFAULT '',
                            serialstatus INTEGER DEFAULT 0,
                            expirydate TEXT NOT NULL DEFAULT '2001-01-01',
                            batchno TEXT NOT NULL DEFAULT '',
                            itemcolor TEXT NOT NULL DEFAULT '',
                            converted INTEGER DEFAULT 0,
                            vattype TEXT NOT NULL DEFAULT '',
                            vatable REAL DEFAULT 0,
                            exempt REAL DEFAULT 0,
                            nonvatable REAL DEFAULT 0,
                            zerorated REAL DEFAULT 0,
                            income_account TEXT NOT NULL DEFAULT '',
                            cogs_account TEXT NOT NULL DEFAULT '',
                            inventory_account TEXT NOT NULL DEFAULT '',
                            job_no TEXT,
                            job_desc TEXT NOT NULL DEFAULT '',
                            name_type TEXT NOT NULL DEFAULT '',
                            docref TEXT NOT NULL DEFAULT '',
                            name_desc TEXT NOT NULL DEFAULT '',
                            length REAL DEFAULT 0.00,
                            width REAL DEFAULT 0.00,
                            area REAL DEFAULT 0.00,
                            perimeter REAL DEFAULT 0.00,
                            principal REAL DEFAULT 0,
                            interest REAL DEFAULT 0,
                            penalty REAL DEFAULT 0,
                            total_loan_amount REAL DEFAULT 0,
                            penalty_rate REAL DEFAULT 0,
                            penalty_term REAL DEFAULT 0,
                            requested REAL DEFAULT 0.00,
                            entry_time TEXT,
                            entry_id TEXT NOT NULL DEFAULT '',
                            entry_status TEXT NOT NULL DEFAULT '',
                            entry_type TEXT NOT NULL DEFAULT '',
                            entry_name TEXT NOT NULL DEFAULT '',
                            entry_date TEXT NOT NULL DEFAULT '',
                            entry_time_zone TEXT NOT NULL DEFAULT '',
                            entry_time_zone_offset TEXT NOT NULL DEFAULT '',
                            entry_time_zone_id TEXT NOT NULL DEFAULT '',
                            entry_time_zone_name TEXT NOT NULL DEFAULT '',
                            entry_time_zone_abbreviation TEXT NOT NULL DEFAULT '',
                            entry_time_zone_display_name TEXT NOT NULL DEFAULT '',
                            entry_time_zone_standard_name TEXT NOT NULL DEFAULT '',
                            entry_time_zone_daylight_name TEXT NOT NULL DEFAULT '',
                            entry_time_zone_supports_daylight_saving_time INTEGER NOT NULL DEFAULT 0,
                            entry_time_zone_base_utc_offset TEXT NOT NULL DEFAULT '',
                            entry_time_zone_adjustment_rules TEXT NOT NULL DEFAULT '',
                            entry_time_zone_has_inconsistent_rules INTEGER NOT NULL DEFAULT 0,
                            entry_time_zone_supports_daylight_saving_time_2 INTEGER NOT NULL DEFAULT 0,
                            entry_time_zone_base_utc_offset_2 TEXT NOT NULL DEFAULT '',
                            entry_time_zone_adjustment_rules_2 TEXT NOT NULL DEFAULT '',
                            entry_time_zone_has_inconsistent_rules_2 INTEGER NOT NULL DEFAULT 0
                        )";
                    await journalBackupCommand.ExecuteNonQueryAsync();

                    // Then copy the data
                    journalBackupCommand.CommandText = $@"
                        ATTACH DATABASE '{sourceJournalDbPath}' AS source;
                        INSERT INTO AccountJournal 
                        SELECT * FROM source.AccountJournal;
                        DETACH DATABASE source;";
                    await journalBackupCommand.ExecuteNonQueryAsync();
                }

                // Truncate tables
                using var truncateOrderCommand = _dataContext.Database.GetDbConnection().CreateCommand();
                truncateOrderCommand.CommandText = "DELETE FROM [Order]";
                await truncateOrderCommand.ExecuteNonQueryAsync();

                using var truncateJournalCommand = _journal.Database.GetDbConnection().CreateCommand();
                truncateJournalCommand.CommandText = "DELETE FROM AccountJournal";
                await truncateJournalCommand.ExecuteNonQueryAsync();

                // Reset auto-increment counters
                using var resetOrderCommand = _dataContext.Database.GetDbConnection().CreateCommand();
                resetOrderCommand.CommandText = "DELETE FROM sqlite_sequence WHERE name='Order'";
                await resetOrderCommand.ExecuteNonQueryAsync();

                using var resetJournalCommand = _journal.Database.GetDbConnection().CreateCommand();
                resetJournalCommand.CommandText = "DELETE FROM sqlite_sequence WHERE name='AccountJournal'";
                await resetJournalCommand.ExecuteNonQueryAsync();

                // Increment the ResetCounterNo
                if (isTrainMode)
                {
                    posInfo.ResetCounterTrainNo += 1;
                }
                else
                {
                    posInfo.ResetCounterNo += 1;
                }
                await _dataContext.SaveChangesAsync();

                return (true, "Order and journal tables backed up and truncated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to truncate orders");
                return (false, $"Failed to truncate orders: {ex.Message}");
            }
        }

        public async Task<(bool isSuccess, string message)> UnpostPwdScAccountJournal(long orderId, string oscaNum)
        {
            var pwdOrSc = await _journal.AccountJournal
                .FirstOrDefaultAsync(x => x.Reference == oscaNum && x.EntryNo == orderId);

            if (pwdOrSc == null)
                return (false, "Not Found Pwd/Sc");

            pwdOrSc.Status = "Unposted";
            await _journal.SaveChangesAsync();

            return (true, "Success");
        }
    }
}
