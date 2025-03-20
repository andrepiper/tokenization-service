using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TokenizationService.Models;
using TokenizationService.Services;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TokenizationService.Data
{
    public class TenantInitializer
    {
        private readonly TokenizationDbContext _dbContext;
        private readonly TenantSettings _tenantSettings;
        private readonly ILogger<TenantInitializer> _logger;

        public TenantInitializer(
            TokenizationDbContext dbContext, 
            IOptions<TenantSettings> tenantSettings,
            ILogger<TenantInitializer> logger)
        {
            _dbContext = dbContext;
            _tenantSettings = tenantSettings.Value;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database is created
                await _dbContext.Database.EnsureCreatedAsync();
                
                _logger.LogInformation("Initializing database and migrating tenants from appsettings.json...");
                
                // Migrate tenants from appsettings.json to database if they don't already exist
                if (_tenantSettings.Tenants != null && _tenantSettings.Tenants.Count > 0)
                {
                    foreach (var tenant in _tenantSettings.Tenants)
                    {
                        var existingTenant = await _dbContext.Tenants
                            .FirstOrDefaultAsync(t => t.Id == tenant.Id);
                            
                        if (existingTenant == null)
                        {
                            _logger.LogInformation($"Migrating tenant {tenant.Id} - {tenant.Name} from appsettings.json to database...");
                            
                            var dbTenant = new DbTenant
                            {
                                Id = tenant.Id,
                                Name = tenant.Name,
                                ApiKey = tenant.ApiKey,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = "system",
                                IsActive = true,
                                IsAdmin = tenant.IsAdmin
                            };
                            
                            // Set encryption settings
                            dbTenant.SetEncryptionSettings(tenant.EncryptionSettings ?? new EncryptionSettings());
                            
                            // Set compliance options
                            dbTenant.SetComplianceOptions(tenant.DefaultComplianceOptions ?? new ComplianceOptions());
                            
                            // Add to database
                            await _dbContext.Tenants.AddAsync(dbTenant);
                            await _dbContext.SaveChangesAsync();
                            
                            _logger.LogInformation($"Tenant {tenant.Id} migrated to database.");
                            
                            if (tenant.IsAdmin)
                            {
                                _logger.LogInformation($"Tenant {tenant.Id} is configured as a system administrator.");
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"Tenant {tenant.Id} already exists in database, skipping migration.");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No tenants found in appsettings.json, skipping tenant migration.");
                    
                    // Check if we need to create default tenants in the database if it's empty
                    var anyTenants = await _dbContext.Tenants.AnyAsync();
                    if (!anyTenants)
                    {
                        _logger.LogInformation("No tenants found in database, creating default admin tenant...");
                        
                        // Create a default admin tenant
                        var defaultTenant = new DbTenant
                        {
                            Id = "admin-tenant",
                            Name = "System Administrator",
                            ApiKey = "admin-api-key-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "system",
                            IsActive = true,
                            IsAdmin = true,
                            EncryptionSettingsJson = JsonSerializer.Serialize(new EncryptionSettings
                            {
                                Algorithm = "AES-256",
                                KeyRotationPolicy = "30Days",
                                MasterKeyReference = "admin-master-key"
                            }),
                            ComplianceOptionsJson = JsonSerializer.Serialize(new ComplianceOptions
                            {
                                EnablePciCompliance = true,
                                EnableHipaaCompliance = true,
                                EnableSoc2Compliance = true,
                                EnableIso27001Compliance = true
                            })
                        };
                        
                        await _dbContext.Tenants.AddAsync(defaultTenant);
                        await _dbContext.SaveChangesAsync();
                        
                        _logger.LogInformation($"Created default admin tenant with ID {defaultTenant.Id} and API key {defaultTenant.ApiKey}");
                    }
                }
                
                // Create a sample token for each tenant
                var dbTenants = await _dbContext.Tenants.ToListAsync();
                foreach (var dbTenant in dbTenants)
                {
                    var existingToken = await _dbContext.Tokens
                        .FirstOrDefaultAsync(t => t.TenantId == dbTenant.Id && t.Type == "sample");

                    if (existingToken == null)
                    {
                        _logger.LogInformation($"Creating sample token for tenant {dbTenant.Id}...");
                        
                        // Make sure ComplianceOptionsJson is not null before deserializing
                        ComplianceOptions complianceOptions;
                        if (string.IsNullOrEmpty(dbTenant.ComplianceOptionsJson))
                        {
                            complianceOptions = new ComplianceOptions();
                            // Update the tenant with default options
                            dbTenant.ComplianceOptionsJson = JsonSerializer.Serialize(complianceOptions);
                            _dbContext.Tenants.Update(dbTenant);
                            await _dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            complianceOptions = dbTenant.GetComplianceOptions();
                        }
                        
                        var metadataDict = new Dictionary<string, string> 
                        { 
                            { "purpose", "initialization" },
                            { "environment", "development" }
                        };
                        
                        var token = new Token
                        {
                            TenantId = dbTenant.Id,
                            Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"Sample data for tenant: {dbTenant.Name}")),
                            Type = "sample",
                            MetadataJson = JsonSerializer.Serialize(metadataDict),
                            EncryptionKeyId = $"{dbTenant.Id}-{Guid.NewGuid()}",
                            IsPci = complianceOptions.EnablePciCompliance,
                            IsHipaa = complianceOptions.EnableHipaaCompliance,
                            IsSoc2 = complianceOptions.EnableSoc2Compliance,
                            IsIso27001 = complianceOptions.EnableIso27001Compliance
                        };
                        
                        token.Metadata = metadataDict;

                        _dbContext.Tokens.Add(token);

                        // Create a sample audit log
                        var auditLog = new AuditLog
                        {
                            TokenId = token.Id,
                            TenantId = dbTenant.Id,
                            Action = "Tenant initialization",
                            Timestamp = DateTime.UtcNow,
                            UserId = "system"
                        };

                        _dbContext.AuditLogs.Add(auditLog);
                        
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation($"Created sample token for tenant {dbTenant.Id}.");
                    }
                }
                
                _logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database initialization");
                throw; // Rethrow to let the app handle it
            }
        }
    }
} 