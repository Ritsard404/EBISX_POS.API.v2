using EBISX_POS.API.Extensions;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Set the base directory for the API
var baseDir = AppContext.BaseDirectory;
Debug.WriteLine($"API Base Directory: {baseDir}");

// Ensure configuration is loaded correctly
builder.Configuration.SetBasePath(baseDir)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure services
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();
builder.WebHost.UseUrls("http://localhost:5166");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Initialize database
await app.InitializeDatabaseAsync();

app.Run();
