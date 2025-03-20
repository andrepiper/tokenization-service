using System.Collections.Generic;
using System.Threading.Tasks;
using TokenizationService.Models;

namespace TokenizationService.Services
{
    public interface ITenantService
    {
        Task<IEnumerable<DbTenant>> GetAllTenantsAsync();
        Task<DbTenant> GetTenantByIdAsync(string tenantId);
        Task<DbTenant> GetTenantByApiKeyAsync(string apiKey);
        Task<DbTenant> CreateTenantAsync(DbTenant tenant);
        Task<DbTenant> UpdateTenantAsync(DbTenant tenant);
        Task<bool> DeleteTenantAsync(string tenantId);
        Task<bool> ValidateApiKeyAsync(string apiKey);
        Task<List<Tenant>> GetTenantsForCompatibilityAsync();
    }
} 