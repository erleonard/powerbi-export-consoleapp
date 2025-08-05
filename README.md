# PowerBI-Export-ConsoleApp

This project demonstrates how to export Power BI reports to PDF format using REST API calls and store them in Azure Blob Storage. This solution is specifically designed for Power BI Premium workspaces, as the export functionality is not available in Pro or PPU licenses.

## Overview

In Power BI Service, we can export reports in PDF format and easily create documents or slides based on Power BI Reports. This project provides two implementation approaches:

1. **.NET Console Application**: A standalone C# application that demonstrates the complete export workflow

Both methods follow the same workflow but differ in their implementation approach and deployment model.

## Export Workflow

The export process consists of the following steps:

1. **Initiate Export**: Call the `Report – Export to File in Group Rest API` with your groupId and reportId to get an exportId.
2. **Check Status**: Call the `Reports-Get Export to File Status in Group Rest API` with the groupId, reportId and exportId to query the current status.
3. **Download PDF**: Once export is complete, call the `Reports-Get File of Export to File in Group Rest API` to get the file data.
4. **Store in Blob Storage**: Upload the PDF file to Azure Blob Storage for storage and sharing.

## .NET Console Application

A comprehensive .NET 8 console application is included in the `/PowerBIExportConsole` directory. This application demonstrates the complete Power BI export workflow using modern C# practices and patterns.

### Features

- **Entra ID Service Principal Authentication**: Secure OAuth2 client credentials flow
- **Complete Export Workflow**: Implements all three Power BI export REST APIs
- **Multiple Export Formats**: Supports PDF, PPTX, and PNG formats
- **Automatic Status Polling**: Monitors export progress with configurable intervals
- **Comprehensive Logging**: Structured logging with multiple levels
- **Flexible Configuration**: Multiple configuration sources (JSON, environment variables, user secrets)
- **Production-Ready**: Error handling, retry logic, and security best practices

### Prerequisites

- .NET 8.0 SDK or later
- Power BI Premium workspace (required for export functionality)
- Azure Entra ID application registration with Power BI API permissions
- Service Principal configured with workspace access

### Quick Start

1. **Clone and navigate to the console application:**
   ```bash
   cd PowerBIExportConsole
   ```

2. **Configure your settings in `appsettings.json`:**
   ```json
   {
     "PowerBI": {
       "TenantId": "your-tenant-id",
       "ClientId": "your-client-id",
       "ClientSecret": "your-client-secret",
       "GroupId": "your-power-bi-workspace-id",
       "ReportId": "your-power-bi-report-id"
     },
     "Export": {
       "Format": "PDF",
       "OutputDirectory": "./exports"
     }
   }
   ```

3. **Build and run:**
   ```bash
   dotnet build
   dotnet run
   ```

### API Implementation

The console application implements the three main Power BI export REST APIs:

1. **Export to File in Group** (`POST /v1.0/myorg/groups/{groupId}/reports/{reportId}/ExportTo`)
   - Initiates the export process
   - Returns an export ID for tracking

2. **Get Export Status in Group** (`GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}`)
   - Polls export progress
   - Returns status and completion percentage

3. **Get File of Export to File in Group** (`GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}/file`)
   - Downloads the completed export file
   - Returns binary file data

### Configuration Options

The application supports flexible configuration through:
- `appsettings.json` files
- Environment variables (e.g., `PowerBI__TenantId`)
- Azure Key Vault (for production deployments)
- User secrets for development

For detailed setup instructions and advanced configuration options, see the [PowerBIExportConsole README](PowerBIExportConsole/README.md).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributors

This project is maintained by:

- **Eric Leonard** ([@erleonard](https://github.com/erleonard)) - Project maintainer and primary contributor
- **Joel Hebert** ([@hebe0022](https://github.com/hebe0022)) - Contributor

**Note:** This is a demonstration project. For production use, ensure you implement appropriate security measures, error handling, and monitoring according to your organization's requirements.