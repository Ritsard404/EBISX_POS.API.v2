using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EBISX_POS.API.Extensions
{
    public static class StartupDBExtensions
    {
        private class DatabaseInitializer
        {
            private readonly ILogger<DatabaseInitializer> _logger;
            private readonly IServiceProvider _services;
            private readonly IConfiguration _configuration;

            public DatabaseInitializer(ILogger<DatabaseInitializer> logger, IServiceProvider services, IConfiguration configuration)
            {
                _logger = logger;
                _services = services;
                _configuration = configuration;
            }

            private async Task InitializeDatabaseAsync<TContext>(TContext context, string databaseName) where TContext : DbContext
            {
                try
                {
                    _logger.LogInformation($"Initializing {databaseName} database...");
                    await context.Database.EnsureCreatedAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error initializing {databaseName} database");
                    throw;
                }
            }

            public async Task InitializeAsync()
            {
                try
                {
                    var dataContext = _services.GetRequiredService<DataContext>();
                    var journalContext = _services.GetRequiredService<JournalContext>();

                    // Initialize both databases
                    await InitializeDatabaseAsync(dataContext, "POS");
                    await InitializeDatabaseAsync(journalContext, "Journal");

                    // Check if we need to seed initial data
                    if (!await dataContext.PosTerminalInfo.AnyAsync())
                    {
                        var terminal = new PosTerminalInfo
                        {
                            PosSerialNumber = "POS-123456789",
                            MinNumber = "MIN-987654321",
                            AccreditationNumber = "ACC-00112233",
                            PtuNumber = "PTU-44556677",
                            DateIssued = new DateTime(2023, 1, 1),
                            ValidUntil = new DateTime(2028, 1, 1),
                            RegisteredName = "EBISX Food Services",
                            OperatedBy = "EBISX Food, Inc.",
                            Address = "123 Main Street, Cebu City",
                            VatTinNumber = "123-456-789-000"
                        };

                        await dataContext.PosTerminalInfo.AddAsync(terminal);
                        await dataContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while initializing the database.");
                    throw;
                }
            }
        }

        public static async Task InitializeDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var initializer = new DatabaseInitializer(
                services.GetRequiredService<ILogger<DatabaseInitializer>>(),
                services,
                services.GetRequiredService<IConfiguration>()
            );
            
            await initializer.InitializeAsync();
        }
    }
}
