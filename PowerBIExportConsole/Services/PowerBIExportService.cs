using Microsoft.Extensions.Logging;
using PowerBIExportConsole.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PowerBIExportConsole.Services;

public interface IPowerBIExportService
{
    Task<string> StartExportAsync(ExportRequest exportRequest);
    Task<ExportResponse> GetExportStatusAsync(string exportId);
    Task<byte[]> GetExportFileAsync(string exportId);
    Task<byte[]> ExportReportToFileAsync(ExportRequest exportRequest);
}

public class PowerBIExportService : IPowerBIExportService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly PowerBIConfiguration _config;
    private readonly ExportConfiguration _exportConfig;
    private readonly ILogger<PowerBIExportService> _logger;

    public PowerBIExportService(
        HttpClient httpClient,
        IAuthenticationService authService,
        PowerBIConfiguration config,
        ExportConfiguration exportConfig,
        ILogger<PowerBIExportService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _config = config;
        _exportConfig = exportConfig;
        _logger = logger;
    }

    public async Task<string> StartExportAsync(ExportRequest exportRequest)
    {
        try
        {
            _logger.LogInformation("Starting export for report {ReportId} in group {GroupId}", 
                _config.ReportId, _config.GroupId);

            var accessToken = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{_config.BaseUrl}/groups/{_config.GroupId}/reports/{_config.ReportId}/ExportTo";
            
            var json = JsonSerializer.Serialize(exportRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending export request to: {Url}", url);
            _logger.LogDebug("Request payload: {Payload}", json);

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Export request failed with status {StatusCode}: {Content}", 
                    response.StatusCode, responseContent);
                
                try
                {
                    var error = JsonSerializer.Deserialize<PowerBIError>(responseContent);
                    throw new HttpRequestException($"Power BI API error: {error?.Error?.Message ?? responseContent}");
                }
                catch (JsonException)
                {
                    throw new HttpRequestException($"Power BI API error: {responseContent}");
                }
            }

            var exportResponse = JsonSerializer.Deserialize<ExportResponse>(responseContent);
            var exportId = exportResponse?.Id ?? throw new InvalidOperationException("Export ID not returned");

            _logger.LogInformation("Export started successfully with ID: {ExportId}", exportId);
            return exportId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start export");
            throw;
        }
    }

    public async Task<ExportResponse> GetExportStatusAsync(string exportId)
    {
        try
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{_config.BaseUrl}/groups/{_config.GroupId}/reports/{_config.ReportId}/exports/{exportId}";
            
            _logger.LogDebug("Checking export status at: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Export status request failed with status {StatusCode}: {Content}", 
                    response.StatusCode, responseContent);
                
                try
                {
                    var error = JsonSerializer.Deserialize<PowerBIError>(responseContent);
                    throw new HttpRequestException($"Power BI API error: {error?.Error?.Message ?? responseContent}");
                }
                catch (JsonException)
                {
                    throw new HttpRequestException($"Power BI API error: {responseContent}");
                }
            }

            var exportResponse = JsonSerializer.Deserialize<ExportResponse>(responseContent) 
                ?? throw new InvalidOperationException("Unable to deserialize export status response");

            _logger.LogDebug("Export {ExportId} status: {Status} ({PercentComplete}% complete)", 
                exportId, exportResponse.Status, exportResponse.PercentComplete);

            return exportResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get export status for {ExportId}", exportId);
            throw;
        }
    }

    public async Task<byte[]> GetExportFileAsync(string exportId)
    {
        try
        {
            _logger.LogInformation("Downloading export file for {ExportId}", exportId);

            var accessToken = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{_config.BaseUrl}/groups/{_config.GroupId}/reports/{_config.ReportId}/exports/{exportId}/file";
            
            _logger.LogDebug("Downloading file from: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Export file download failed with status {StatusCode}: {Content}", 
                    response.StatusCode, responseContent);
                
                try
                {
                    var error = JsonSerializer.Deserialize<PowerBIError>(responseContent);
                    throw new HttpRequestException($"Power BI API error: {error?.Error?.Message ?? responseContent}");
                }
                catch (JsonException)
                {
                    throw new HttpRequestException($"Power BI API error: {responseContent}");
                }
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation("Successfully downloaded {FileSize} bytes for export {ExportId}", 
                fileBytes.Length, exportId);

            return fileBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download export file for {ExportId}", exportId);
            throw;
        }
    }

    public async Task<byte[]> ExportReportToFileAsync(ExportRequest exportRequest)
    {
        // Start the export
        var exportId = await StartExportAsync(exportRequest);

        // Poll for completion
        var maxWaitTime = TimeSpan.FromMinutes(_exportConfig.MaxWaitTimeMinutes);
        var pollingInterval = TimeSpan.FromSeconds(_exportConfig.PollingIntervalSeconds);
        var startTime = DateTime.UtcNow;

        ExportResponse exportStatus;
        do
        {
            if (DateTime.UtcNow - startTime > maxWaitTime)
            {
                throw new TimeoutException($"Export {exportId} did not complete within {_exportConfig.MaxWaitTimeMinutes} minutes");
            }

            await Task.Delay(pollingInterval);
            exportStatus = await GetExportStatusAsync(exportId);

            _logger.LogInformation("Export {ExportId} progress: {Status} ({PercentComplete}%)", 
                exportId, exportStatus.Status, exportStatus.PercentComplete);

        } while (exportStatus.Status.Equals("Running", StringComparison.OrdinalIgnoreCase) || 
                 exportStatus.Status.Equals("NotStarted", StringComparison.OrdinalIgnoreCase));

        if (!exportStatus.Status.Equals("Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Export {exportId} failed with status: {exportStatus.Status}");
        }

        // Download the file
        return await GetExportFileAsync(exportId);
    }
}
