using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerBIExportConsole.Models;
using PowerBIExportConsole.Services;

namespace PowerBIExportConsole;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        try
        {
            await host.Services.GetRequiredService<PowerBIExportApplication>().RunAsync();
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogCritical(ex, "Application terminated unexpectedly");
            Environment.Exit(1);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddUserSecrets<Program>();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Bind configuration
                var powerBIConfig = new PowerBIConfiguration();
                configuration.GetSection("PowerBI").Bind(powerBIConfig);
                services.AddSingleton(powerBIConfig);

                var exportConfig = new ExportConfiguration();
                configuration.GetSection("Export").Bind(exportConfig);
                services.AddSingleton(exportConfig);

                // Register services
                services.AddHttpClient();
                services.AddSingleton<IAuthenticationService, AuthenticationService>();
                services.AddSingleton<IPowerBIExportService, PowerBIExportService>();
                services.AddSingleton<PowerBIExportApplication>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
            });
}

public class PowerBIExportApplication
{
    private readonly IPowerBIExportService _exportService;
    private readonly ExportConfiguration _exportConfig;
    private readonly PowerBIConfiguration _powerBIConfig;
    private readonly ILogger<PowerBIExportApplication> _logger;

    public PowerBIExportApplication(
        IPowerBIExportService exportService,
        ExportConfiguration exportConfig,
        PowerBIConfiguration powerBIConfig,
        ILogger<PowerBIExportApplication> logger)
    {
        _exportService = exportService;
        _exportConfig = exportConfig;
        _powerBIConfig = powerBIConfig;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Power BI Export Console Application");
        
        // Validate configuration
        ValidateConfiguration();

        try
        {
            // Create export request
            var exportRequest = new ExportRequest
            {
                Format = _exportConfig.Format.ToUpperInvariant()
            };

            _logger.LogInformation("Exporting report {ReportId} from group {GroupId} to {Format} format", 
                _powerBIConfig.ReportId, _powerBIConfig.GroupId, exportRequest.Format);

            // Export the report
            var fileBytes = await _exportService.ExportReportToFileAsync(exportRequest);

            // Save the file
            var fileName = await SaveExportedFileAsync(fileBytes, exportRequest.Format);
            
            _logger.LogInformation("Export completed successfully. File saved as: {FileName}", fileName);
            Console.WriteLine($"✅ Export completed successfully!");
            Console.WriteLine($"📁 File saved as: {fileName}");
            Console.WriteLine($"📊 File size: {fileBytes.Length:N0} bytes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed: {Error}", ex.Message);
            Console.WriteLine($"❌ Export failed: {ex.Message}");
            throw;
        }
    }

    private void ValidateConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_powerBIConfig.TenantId))
            errors.Add("PowerBI:TenantId is required");
        
        if (string.IsNullOrWhiteSpace(_powerBIConfig.ClientId))
            errors.Add("PowerBI:ClientId is required");
        
        if (string.IsNullOrWhiteSpace(_powerBIConfig.ClientSecret))
            errors.Add("PowerBI:ClientSecret is required");
        
        if (string.IsNullOrWhiteSpace(_powerBIConfig.GroupId))
            errors.Add("PowerBI:GroupId is required");
        
        if (string.IsNullOrWhiteSpace(_powerBIConfig.ReportId))
            errors.Add("PowerBI:ReportId is required");

        if (!IsValidFormat(_exportConfig.Format))
            errors.Add($"Export:Format '{_exportConfig.Format}' is not valid. Supported formats: PDF, PPTX, PNG");

        if (errors.Any())
        {
            var errorMessage = "Configuration validation failed:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));
            _logger.LogError("Configuration validation failed: {Errors}", string.Join(", ", errors));
            throw new InvalidOperationException(errorMessage);
        }

        _logger.LogInformation("Configuration validation passed");
    }

    private static bool IsValidFormat(string format)
    {
        var validFormats = new[] { "PDF", "PPTX", "PNG" };
        return validFormats.Contains(format.ToUpperInvariant());
    }

    private async Task<string> SaveExportedFileAsync(byte[] fileBytes, string format)
    {
        // Ensure output directory exists
        var outputDir = Path.GetFullPath(_exportConfig.OutputDirectory);
        Directory.CreateDirectory(outputDir);

        // Generate filename with timestamp
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var extension = format.ToLowerInvariant();
        var fileName = $"powerbi_export_{timestamp}.{extension}";
        var filePath = Path.Combine(outputDir, fileName);

        // Save the file
        await File.WriteAllBytesAsync(filePath, fileBytes);
        
        _logger.LogInformation("File saved to: {FilePath}", filePath);
        return filePath;
    }
}
