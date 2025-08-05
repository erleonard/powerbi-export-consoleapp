# Power BI Export Console Application

A .NET console application that demonstrates how to export Power BI reports to files using the Power BI REST API with Entra ID Service Principal authentication.

## Features

- **Entra ID Service Principal Authentication**: Secure authentication using client credentials flow
- **Power BI Export API Integration**: Implements the complete export workflow:
  - Export to File in Group
  - Get Export Status in Group
  - Get File of Export to File in Group
- **Multiple Export Formats**: Supports PDF, PPTX, and PNG formats
- **Automatic Status Polling**: Monitors export progress until completion
- **Comprehensive Logging**: Detailed logging for troubleshooting and monitoring
- **Flexible Configuration**: Supports multiple configuration sources

## Prerequisites

- .NET 8.0 or later
- Power BI Premium workspace (required for export functionality)
- Azure Entra ID application with appropriate permissions
- Power BI service principal configured with workspace access

## Setup

### 1. Azure Entra ID Application Setup

1. Register a new application in Azure Entra ID
2. Create a client secret
3. Grant the following API permissions:
   - Power BI Service: `Tenant.Read.All`, `Report.Read.All`, `Workspace.Read.All`
4. Note down:
   - Tenant ID
   - Application (Client) ID
   - Client Secret

### 2. Power BI Service Configuration

1. Enable service principal access in Power BI Admin Portal:
   - Go to Admin Portal > Tenant Settings
   - Enable "Allow service principals to use Power BI APIs"
   - Add your service principal to the allowed security group

2. Add service principal to workspace:
   - Go to your Premium workspace
   - Add the service principal as Admin or Member

### 3. Application Configuration

Update `appsettings.json` with your values:

```json
{
  "PowerBI": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id", 
    "ClientSecret": "your-client-secret",
    "GroupId": "your-group-id",
    "ReportId": "your-report-id"
  }
}
```

Alternatively, use environment variables:
- `PowerBI__TenantId`
- `PowerBI__ClientId`
- `PowerBI__ClientSecret`
- `PowerBI__GroupId`
- `PowerBI__ReportId`

## Usage

### Build and Run

```bash
cd PowerBIExportConsole
dotnet build
dotnet run
```

### Configuration Options

The application supports the following configuration sections:

#### PowerBI Configuration
- `TenantId`: Azure Entra ID tenant ID
- `ClientId`: Application (client) ID
- `ClientSecret`: Client secret
- `Scope`: OAuth scope (default: `https://analysis.windows.net/powerbi/api/.default`)
- `BaseUrl`: Power BI API base URL (default: `https://api.powerbi.com/v1.0/myorg`)
- `GroupId`: Power BI workspace ID
- `ReportId`: Power BI report ID

#### Export Configuration
- `Format`: Export format - PDF, PPTX, or PNG (default: PDF)
- `PollingIntervalSeconds`: Status polling interval (default: 5)
- `MaxWaitTimeMinutes`: Maximum wait time for export completion (default: 10)
- `OutputDirectory`: Directory for exported files (default: ./exports)

## API Implementation

The application implements the three main Power BI export APIs:

### 1. Export to File in Group
```
POST /v1.0/myorg/groups/{groupId}/reports/{reportId}/ExportTo
```
Initiates the export process and returns an export ID.

### 2. Get Export Status in Group
```
GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}
```
Checks the status and progress of the export operation.

### 3. Get File of Export to File in Group
```
GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}/file
```
Downloads the exported file once the export is complete.

## Error Handling

The application includes comprehensive error handling for:
- Authentication failures
- API rate limiting
- Export timeouts
- Invalid configurations
- Network connectivity issues

## Logging

Structured logging is implemented using Microsoft.Extensions.Logging with configurable log levels:
- Information: General application flow
- Debug: Detailed diagnostic information
- Warning: Potentially harmful situations
- Error: Error events that allow the application to continue
- Critical: Serious error events

## Security Best Practices

- Client secrets are loaded from secure configuration sources
- Access tokens are acquired on-demand and not stored
- HTTP client follows recommended patterns for authentication
- Sensitive information is not logged

## Limitations

- Requires Power BI Premium workspace (not available for Pro/PPU)
- Export functionality has API rate limits
- Large reports may take significant time to export
- File size limits apply based on Power BI service constraints

## Troubleshooting

### Common Issues

1. **Authentication Failed**: Verify tenant ID, client ID, and client secret
2. **Forbidden Access**: Ensure service principal has workspace permissions
3. **Report Not Found**: Verify group ID and report ID are correct
4. **Export Timeout**: Increase `MaxWaitTimeMinutes` for large reports
5. **API Limits**: Implement retry logic for production use

### Getting IDs

- **Group ID**: Available in workspace settings URL or Power BI REST API
- **Report ID**: Available in report URL or Power BI REST API

## Dependencies

- Microsoft.Identity.Client: MSAL for authentication
- Microsoft.Extensions.*: Configuration, logging, and dependency injection
- System.Text.Json: JSON serialization

## License

This project is licensed under the MIT License. See the LICENSE file for details.
