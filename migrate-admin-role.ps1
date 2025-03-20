# Script to add IsAdmin column to database and create admin tenant

param(
    [string]$Server = "localhost",
    [string]$Port = "3306",
    [string]$User = "root",
    [string]$Password = "",
    [string]$Database = "tokenization",
    [string]$ScriptPath = "migrate-admin-role.sql"
)

Write-Host "Migrating database to add admin role support..." -ForegroundColor Green

# Check if mysql.exe is available
try {
    Get-Command mysql -ErrorAction Stop | Out-Null
    $mysqlAvailable = $true
}
catch {
    $mysqlAvailable = $false
    Write-Host "MySQL command-line client not found in PATH. Will generate commands for manual execution." -ForegroundColor Yellow
}

# Construct connection string
$connectionString = "-h$Server -P$Port -u$User"
if ($Password) {
    $connectionString += " -p$Password"
}

if ($mysqlAvailable) {
    # Run the script directly
    try {
        Write-Host "Running SQL script: $ScriptPath" -ForegroundColor Cyan
        Get-Content $ScriptPath | mysql $connectionString

        Write-Host "Database migration completed successfully!" -ForegroundColor Green
        Write-Host "The Tenants table now has an IsAdmin column and an admin tenant has been created (or updated)." -ForegroundColor Green
        Write-Host "You can now use the admin API key to access tenant management endpoints." -ForegroundColor Green
    }
    catch {
        Write-Host "Error executing SQL script: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    # Generate commands for manual execution
    Write-Host "Please run the following commands manually to migrate the database:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Open a terminal or command prompt" -ForegroundColor Cyan
    Write-Host "2. Navigate to the directory containing your MySQL client (if not in PATH)" -ForegroundColor Cyan
    Write-Host "3. Run the following command:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   mysql $connectionString < `"$ScriptPath`"" -ForegroundColor White
    Write-Host ""
    Write-Host "4. If prompted, enter your MySQL password" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "After running the commands, the admin role support will be added to the database." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Admin tenant details:" -ForegroundColor Cyan
Write-Host "  - ID: admin-tenant" -ForegroundColor White
Write-Host "  - API Key: admin-api-key-secure" -ForegroundColor White
Write-Host ""
Write-Host "Use these credentials to access tenant management endpoints." -ForegroundColor Cyan 