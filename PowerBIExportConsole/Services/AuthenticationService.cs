using Microsoft.Identity.Client;
using Microsoft.Extensions.Logging;
using PowerBIExportConsole.Models;

namespace PowerBIExportConsole.Services;

public interface IAuthenticationService
{
    Task<string> GetAccessTokenAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly PowerBIConfiguration _config;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfidentialClientApplication _app;

    public AuthenticationService(PowerBIConfiguration config, ILogger<AuthenticationService> logger)
    {
        _config = config;
        _logger = logger;

        _app = ConfidentialClientApplicationBuilder
            .Create(_config.ClientId)
            .WithClientSecret(_config.ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{_config.TenantId}"))
            .Build();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            _logger.LogInformation("Acquiring access token for Power BI API");

            var result = await _app.AcquireTokenForClient(new[] { _config.Scope })
                .ExecuteAsync();

            _logger.LogInformation("Successfully acquired access token");
            return result.AccessToken;
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "Failed to acquire access token: {Error}", ex.Message);
            throw new InvalidOperationException($"Failed to authenticate: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication: {Error}", ex.Message);
            throw;
        }
    }
}
