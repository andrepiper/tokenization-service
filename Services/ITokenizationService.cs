using System.Collections.Generic;
using System.Threading.Tasks;
using TokenizationService.Models;

namespace TokenizationService.Services
{
    public interface ITokenizationService
    {
        Task<Token> TokenizeAsync(TokenizationRequest request, string tenantId, string userId = "system");
        Task<string> DetokenizeAsync(string tokenId, string tenantId, string userId = "system");
        Task<Token> GetTokenAsync(string tokenId, string tenantId, string userId = "system");
        Task<bool> DeleteTokenAsync(string tokenId, string tenantId, string userId = "system");
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string tokenId, string tenantId, string userId = "system");
    }
} 