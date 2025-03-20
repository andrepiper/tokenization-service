using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TokenizationService.Data;
using TokenizationService.Models;

namespace TokenizationService.Services
{
    public class TenantService : ITenantService
    {
        private readonly TokenizationDbContext _dbContext;
        private readonly ILogger<TenantService> _logger;

        public TenantService(TokenizationDbContext dbContext, ILogger<TenantService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<DbTenant>> GetAllTenantsAsync()
        {
            return await _dbContext.Tenants
                .Where(t => t.IsActive)
                .ToListAsync();
        }

        public async Task<DbTenant> GetTenantByIdAsync(string tenantId)
        {
            return await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
        }

        public async Task<DbTenant> GetTenantByApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return null;
                
            try {
                return await _dbContext.Tenants
                    .FirstOrDefaultAsync(t => t.ApiKey == apiKey && t.IsActive);
            }
            catch (InvalidCastException ex)
            {
                _logger.LogError(ex, $"Error fetching tenant with API key: Database type conversion error");
                return null;
            }
        }

        public async Task<DbTenant> CreateTenantAsync(DbTenant tenant)
        {
            // Ensure tenant doesn't already exist
            var existingTenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenant.Id);
                
            if (existingTenant != null)
            {
                throw new InvalidOperationException($"Tenant with ID {tenant.Id} already exists");
            }
            
            // Check for duplicate API key
            var existingApiKey = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.ApiKey == tenant.ApiKey);
                
            if (existingApiKey != null)
            {
                throw new InvalidOperationException($"Tenant with API key {tenant.ApiKey} already exists");
            }
            
            tenant.CreatedAt = DateTime.UtcNow;
            tenant.IsActive = true;
            
            await _dbContext.Tenants.AddAsync(tenant);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation($"Created tenant {tenant.Id} - {tenant.Name}");
            
            return tenant;
        }

        public async Task<DbTenant> UpdateTenantAsync(DbTenant tenant)
        {
            var existingTenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenant.Id);
                
            if (existingTenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenant.Id} not found");
            }
            
            // Check for duplicate API key if changed
            if (tenant.ApiKey != existingTenant.ApiKey)
            {
                var existingApiKey = await _dbContext.Tenants
                    .FirstOrDefaultAsync(t => t.ApiKey == tenant.ApiKey && t.Id != tenant.Id);
                    
                if (existingApiKey != null)
                {
                    throw new InvalidOperationException($"Tenant with API key {tenant.ApiKey} already exists");
                }
            }
            
            // Update properties
            existingTenant.Name = tenant.Name;
            existingTenant.ApiKey = tenant.ApiKey;
            existingTenant.EncryptionSettingsJson = tenant.EncryptionSettingsJson;
            existingTenant.ComplianceOptionsJson = tenant.ComplianceOptionsJson;
            existingTenant.LastModifiedAt = DateTime.UtcNow;
            existingTenant.LastModifiedBy = tenant.LastModifiedBy ?? "system";
            existingTenant.IsActive = tenant.IsActive;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation($"Updated tenant {tenant.Id} - {tenant.Name}");
            
            return existingTenant;
        }

        public async Task<bool> DeleteTenantAsync(string tenantId)
        {
            var existingTenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId);
                
            if (existingTenant == null)
            {
                return false;
            }
            
            // Soft delete - set inactive
            existingTenant.IsActive = false;
            existingTenant.LastModifiedAt = DateTime.UtcNow;
            existingTenant.LastModifiedBy = "system";
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation($"Deleted tenant {tenantId}");
            
            return true;
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            return await _dbContext.Tenants
                .AnyAsync(t => t.ApiKey == apiKey && t.IsActive);
        }

        public async Task<List<Tenant>> GetTenantsForCompatibilityAsync()
        {
            var dbTenants = await GetAllTenantsAsync();
            return dbTenants.Select(t => t.ToTenant()).ToList();
        }
    }
} 