using EBISX_POS.API.Data;
using EBISX_POS.API.Services;
using EBISX_POS.API.Services.Interfaces;
using EBISX_POS.API.Services.Repositories;
using EBISX_POS.API.Settings;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EBISX_POS.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            Debug.WriteLine("Configuring application services...");
            Debug.WriteLine($"Base Directory: {AppContext.BaseDirectory}");

            // Add configuration services
            var filePaths = new FilePaths
            {
                ImagePath = Path.Combine(AppContext.BaseDirectory, "Images"),
                BackUp = Path.Combine(AppContext.BaseDirectory, "Backups")
            };

            // Try to get configuration from appsettings.json
            var configFilePaths = configuration.GetSection("FilePaths").Get<FilePaths>();
            if (configFilePaths != null)
            {
                Debug.WriteLine("Found FilePaths in configuration");
                if (!string.IsNullOrEmpty(configFilePaths.ImagePath))
                {
                    filePaths.ImagePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configFilePaths.ImagePath));
                }
                if (!string.IsNullOrEmpty(configFilePaths.BackUp))
                {
                    filePaths.BackUp = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configFilePaths.BackUp));
                }
            }

            Debug.WriteLine($"Final FilePaths configuration:");
            Debug.WriteLine($"ImagePath: {filePaths.ImagePath}");
            Debug.WriteLine($"BackUp: {filePaths.BackUp}");

            // Ensure directories exist
            Directory.CreateDirectory(filePaths.ImagePath);
            Directory.CreateDirectory(filePaths.BackUp);

            services.Configure<FilePaths>(options =>
            {
                options.ImagePath = filePaths.ImagePath;
                options.BackUp = filePaths.BackUp;
            });

            // Register database contexts
            services.AddDatabaseContexts(configuration);

            // Register repositories
            services.AddRepositories();

            // Add CORS with any origin (without credentials)
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            services.AddControllers();
            services.AddSwaggerGen();
            services.AddLogging();
            services.AddOptions();

            return services;
        }

        private static IServiceCollection AddDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
        {
            var posConnectionString = configuration.GetConnectionString("POSConnection");
            var journalConnectionString = configuration.GetConnectionString("JournalConnection");

            Debug.WriteLine($"POS Connection String: {posConnectionString}");
            Debug.WriteLine($"Journal Connection String: {journalConnectionString}");

            services.AddDbContext<DataContext>(options =>
                options.UseSqlite(posConnectionString));

            services.AddDbContext<JournalContext>(options =>
                options.UseSqlite(journalConnectionString));

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAuth, AuthRepository>();
            services.AddScoped<IMenu, MenuRepository>();
            services.AddScoped<IOrder, OrderRepository>();
            services.AddScoped<IPayment, PaymentRepository>();
            services.AddScoped<IJournal, JournalRepository>();
            services.AddScoped<IEbisxAPI, EbisxAPIRepository>();
            services.AddScoped<IReport, ReportRepository>();
            services.AddScoped<IInvoiceNumberService, InvoiceNumberService>();
            services.AddScoped<IData, DataRepository>();

            return services;
        }
    }
}