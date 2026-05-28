using LinuxFileHandler.Configurations;
using Serilog;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

#region Serilog Configuration

// Configure Serilog
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.Enrich.WithMachineName()
	.Enrich.WithThreadId()
	//.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
	.MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
	.MinimumLevel.Information()
	.WriteTo.Console()
	.WriteTo.File(
	"Logs/log-.txt",
	fileSizeLimitBytes: 10 * 1024 * 1024, // 10 MB maximum file size to keep in logs
	rollOnFileSizeLimit: true,
	rollingInterval: RollingInterval.Day,
	retainedFileCountLimit: 10)
	.CreateLogger();

// Replace default logging
builder.Host.UseSerilog();

Log.Information("Application starting...");

// Add services to the container.
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));

#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/tks", (int n) =>
{
	var ticks = DateTime.UtcNow.AddDays(n).Ticks;
	return Results.Ok(new { Ticks = ticks, AppliedDateTime = new DateTime(ticks) });
}).ExcludeFromDescription();

// To check ticks datetime
//app.MapGet("/tksdt", (long tks) =>
//{	
//	return Results.Ok(new { AppliedDateTime = new DateTime(tks) });
//}).ExcludeFromDescription();

app.MapGet("/gkey", () =>
{
	const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
	const int length = 16;
	var result = new char[length];
	using var rng = RandomNumberGenerator.Create();
	byte[] buffer = new byte[length];
	rng.GetBytes(buffer);
	for (int i = 0; i < length; i++)
		result[i] = chars[buffer[i] % chars.Length];

	var key = new string(result);
	return Results.Ok(new { Key = key });
}).ExcludeFromDescription();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
