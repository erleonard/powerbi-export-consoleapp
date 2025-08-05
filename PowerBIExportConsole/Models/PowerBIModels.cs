using System.Text.Json.Serialization;

namespace PowerBIExportConsole.Models;

public class PowerBIConfiguration
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
}

public class ExportConfiguration
{
    public string Format { get; set; } = "PDF";
    public int PollingIntervalSeconds { get; set; } = 5;
    public int MaxWaitTimeMinutes { get; set; } = 10;
    public string OutputDirectory { get; set; } = "./exports";
}

public class ExportRequest
{
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("powerBIReportConfiguration")]
    public PowerBIReportConfiguration? PowerBIReportConfiguration { get; set; }
}

public class PowerBIReportConfiguration
{
    [JsonPropertyName("reportLevelFilters")]
    public List<ReportFilter>? ReportLevelFilters { get; set; }

    [JsonPropertyName("pages")]
    public List<ExportReportPage>? Pages { get; set; }

    [JsonPropertyName("identities")]
    public List<EffectiveIdentity>? Identities { get; set; }
}

public class ReportFilter
{
    [JsonPropertyName("filter")]
    public string Filter { get; set; } = string.Empty;
}

public class ExportReportPage
{
    [JsonPropertyName("pageName")]
    public string PageName { get; set; } = string.Empty;

    [JsonPropertyName("visualName")]
    public string? VisualName { get; set; }

    [JsonPropertyName("bookmark")]
    public PageBookmark? Bookmark { get; set; }
}

public class PageBookmark
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class EffectiveIdentity
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }

    [JsonPropertyName("datasets")]
    public List<string>? Datasets { get; set; }
}

public class ExportResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("createdDateTime")]
    public DateTime CreatedDateTime { get; set; }

    [JsonPropertyName("lastActionDateTime")]
    public DateTime LastActionDateTime { get; set; }

    [JsonPropertyName("reportId")]
    public string ReportId { get; set; } = string.Empty;

    [JsonPropertyName("reportName")]
    public string ReportName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("percentComplete")]
    public int PercentComplete { get; set; }

    [JsonPropertyName("resourceLocation")]
    public string? ResourceLocation { get; set; }

    [JsonPropertyName("expirationTime")]
    public DateTime? ExpirationTime { get; set; }
}

public class PowerBIError
{
    [JsonPropertyName("error")]
    public ErrorDetails Error { get; set; } = new();
}

public class ErrorDetails
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public List<ErrorDetail>? Details { get; set; }
}

public class ErrorDetail
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public enum ExportStatus
{
    NotStarted,
    Running,
    Succeeded,
    Failed
}
