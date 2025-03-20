param (
    [Parameter(Mandatory=$false)]
    [string]$TenantId = "tenant3",
    
    [Parameter(Mandatory=$false)]
    [string]$TenantName = "Third Tenant",
    
    [Parameter(Mandatory=$false)]
    [string]$MysqlUser = "root",
    
    [Parameter(Mandatory=$false)]
    [string]$MysqlPassword = "",
    
    [Parameter(Mandatory=$false)]
    [string]$MysqlHost = "127.0.0.1",
    
    [Parameter(Mandatory=$false)]
    [int]$MysqlPort = 3306
)

# First, run the application to ensure the database and tables are created
Write-Host "Starting the application to initialize the database schema..."
$appProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -NoNewWindow

# Give the application time to initialize the database
Write-Host "Waiting for database initialization..."
Start-Sleep -Seconds 10

# Stop the application
Write-Host "Stopping the application..."
Stop-Process -Id $appProcess.Id -Force

# Now, update the SQL script with the tenant info
$sqlScript = Get-Content -Path "add-tenant.sql" -Raw
$sqlScript = $sqlScript.Replace("tenant3", $TenantId).Replace("Third Tenant", $TenantName)
Set-Content -Path "add-tenant-temp.sql" -Value $sqlScript

# Run the SQL script with the mysql client
Write-Host "Running SQL script to create tenant $TenantId - $TenantName..."
if ([string]::IsNullOrEmpty($MysqlPassword)) {
    # No password
    $command = "mysql -u$MysqlUser -h$MysqlHost -P$MysqlPort < add-tenant-temp.sql"
}
else {
    # With password
    $command = "mysql -u$MysqlUser -p$MysqlPassword -h$MysqlHost -P$MysqlPort < add-tenant-temp.sql"
}

# Execute the MySQL command
Invoke-Expression $command

# Clean up
Remove-Item -Path "add-tenant-temp.sql" -Force -ErrorAction SilentlyContinue

Write-Host "Tenant $TenantId - $TenantName has been created in the database."
Write-Host "You should also update the appsettings.json file to add the tenant configuration."

# Update the appsettings.json file with the new tenant
$apiKey = "$TenantId-api-key"
Write-Host "Updating appsettings.json with the new tenant configuration..."
$appSettings = Get-Content -Path "appsettings.json" -Raw | ConvertFrom-Json

# Add the new tenant if it doesn't exist
$tenantExists = $false
foreach ($tenant in $appSettings.TenantSettings.Tenants) {
    if ($tenant.Id -eq $TenantId) {
        $tenantExists = $true
        break
    }
}

if (-not $tenantExists) {
    $newTenant = @{
        Id = $TenantId
        Name = $TenantName
        ApiKey = $apiKey
        EncryptionSettings = @{
            Algorithm = "AES-256"
            KeyRotationPolicy = "90Days"
            MasterKeyReference = "$TenantId-master-key"
        }
        DefaultComplianceOptions = @{
            EnablePciCompliance = $true
            EnableHipaaCompliance = $true
            EnableSoc2Compliance = $true
            EnableIso27001Compliance = $true
        }
    }
    
    $appSettings.TenantSettings.Tenants += $newTenant
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content -Path "appsettings.json"
    
    Write-Host "Added tenant $TenantId to appsettings.json with API key: $apiKey"
}
else {
    Write-Host "Tenant $TenantId already exists in appsettings.json"
}

Write-Host "Setup complete. You can now run the application with 'dotnet run'." 