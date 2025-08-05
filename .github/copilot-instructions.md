# Azure Logic App Demo Project Guidelines

## Project Overview
This project demonstrates how to use Azure Logic Apps to automate the export of Power BI reports to PDF format and store them in Azure Blob Storage. This solution is specifically designed for Power BI Premium workspaces, as the export functionality is not available in Pro or PPU licenses.

## Architecture

### Main Components
- **Azure Logic App**: Orchestrates the API calls and workflow
- **Power BI REST API**: Used for exporting reports to PDF
- **Azure Blob Storage**: Destination for exported PDF files

### Data Flow
1. Logic App initiates the Power BI export process via REST API
2. Logic App monitors export status until completion
3. Logic App retrieves the exported PDF file
4. Logic App stores the PDF in Azure Blob Storage

## Key Configuration Requirements

### Power BI Configuration
- **Premium Workspace**: This solution only works with Power BI Premium workspaces
- **Group ID**: The workspace ID in Power BI
- **Report ID**: The specific report ID to be exported

### Azure Configuration
- **Service Principal**: Requires proper permissions to access Power BI API
- **Blob Storage**: Configured storage account for PDF destination

## API Workflow
The solution uses three main Power BI API endpoints:

1. **Export to File API**: `POST /v1.0/myorg/groups/{groupId}/reports/{reportId}/ExportTo`
   - Initiates the export and returns an `exportId`

2. **Export Status API**: `GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}`
   - Monitors the export progress until completion

3. **Get File API**: `GET /v1.0/myorg/groups/{groupId}/reports/{reportId}/exports/{exportId}/file`
   - Downloads the exported PDF file

## Development Workflow
1. Set up your Azure Logic App in the Azure Portal
2. Configure Power BI connections with appropriate permissions
3. Implement the step-by-step API workflow as outlined in the README
4. Configure Blob Storage connection for the exported files
5. Test the workflow with various report sizes and monitor performance

## Authentication
The solution uses OAuth Bearer tokens for authentication with the Power BI API. Ensure the service principal has the appropriate Power BI permissions.

## Best Practices
- Implement proper error handling for API failures
- Consider rate limiting and timeout configurations
- Add monitoring and alerting for production deployments
- Use Azure Key Vault for storing sensitive connection strings and credentials
