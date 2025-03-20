param (
    [Parameter(Mandatory=$true)]
    [string]$TenantId,
    
    [Parameter(Mandatory=$true)]
    [string]$TenantName,
    
    [Parameter(Mandatory=$true)]
    [string]$ApiKey
)

# Build and run CreateTenant.csproj with the provided parameters
dotnet build CreateTenant.csproj
Write-Host "Creating tenant: $TenantId - $TenantName with API key: $ApiKey"
dotnet run --project CreateTenant.csproj -- $TenantId $TenantName $ApiKey

# Restart the main application to apply changes
Write-Host "Tenant created. You may need to restart the application to apply changes." 