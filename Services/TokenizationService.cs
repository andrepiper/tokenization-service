using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TokenizationService.Data;
using TokenizationService.Models;

namespace TokenizationService.Services
{
    public class TokenizationService : ITokenizationService
    {
        private readonly TokenizationDbContext _dbContext;
        private readonly ITenantService _tenantService;

        public TokenizationService(TokenizationDbContext dbContext, ITenantService tenantService)
        {
            _dbContext = dbContext;
            _tenantService = tenantService;
        }

        public async Task<Token> TokenizeAsync(TokenizationRequest request, string tenantId, string userId = "system")
        {
            var dbTenant = await _tenantService.GetTenantByIdAsync(tenantId);
            if (dbTenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            }
            
            var tenant = dbTenant.ToTenant();
            
            // If compliance options are not specified in the request, use tenant defaults
            if (request.ComplianceOptions == null)
            {
                request.ComplianceOptions = tenant.DefaultComplianceOptions;
            }
            
            var token = new Token
            {
                TenantId = tenantId,
                Data = EncryptData(request.Data, tenantId, dbTenant, out string keyId),
                Type = request.Type,
                Metadata = request.Metadata,
                ExpiresAt = request.ExpiresAt,
                EncryptionKeyId = keyId,
                IsPci = request.ComplianceOptions.EnablePciCompliance,
                IsHipaa = request.ComplianceOptions.EnableHipaaCompliance,
                IsSoc2 = request.ComplianceOptions.EnableSoc2Compliance,
                IsIso27001 = request.ComplianceOptions.EnableIso27001Compliance
            };

            if (request.GenerateFingerprint)
            {
                token.Fingerprint = GenerateFingerprint(request.Data);
            }

            // Serialize metadata to JSON for storage
            token.SerializeMetadata();

            // Add token to database
            await _dbContext.Tokens.AddAsync(token);
            
            // Log the event
            await LogAuditEventAsync(token.Id, tenantId, "Token created", userId);
            
            // Save changes
            await _dbContext.SaveChangesAsync();

            return token;
        }

        public async Task<string> DetokenizeAsync(string tokenId, string tenantId, string userId = "system")
        {
            var dbTenant = await _tenantService.GetTenantByIdAsync(tenantId);
            if (dbTenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            }
            
            var token = await _dbContext.Tokens
                .FirstOrDefaultAsync(t => t.Id == tokenId && t.TenantId == tenantId);
                
            if (token == null)
            {
                throw new KeyNotFoundException($"Token with ID {tokenId} not found for tenant {tenantId}");
            }

            // Update access info
            token.LastAccessedAt = DateTime.UtcNow;
            token.LastAccessedBy = userId;
            
            // Log the event
            await LogAuditEventAsync(tokenId, tenantId, "Token data accessed", userId);
            
            // Save changes
            await _dbContext.SaveChangesAsync();
            
            // Decrypt and return data
            return DecryptData(token.Data, tenantId, dbTenant, token.EncryptionKeyId);
        }

        public async Task<Token> GetTokenAsync(string tokenId, string tenantId, string userId = "system")
        {
            // Verify tenant exists
            var dbTenant = await _tenantService.GetTenantByIdAsync(tenantId);
            if (dbTenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            }
            
            var token = await _dbContext.Tokens
                .FirstOrDefaultAsync(t => t.Id == tokenId && t.TenantId == tenantId);
                
            if (token == null)
            {
                throw new KeyNotFoundException($"Token with ID {tokenId} not found for tenant {tenantId}");
            }

            // Deserialize metadata from JSON
            token.DeserializeMetadata();
            
            // Update access info
            token.LastAccessedAt = DateTime.UtcNow;
            token.LastAccessedBy = userId;
            
            // Log the event
            await LogAuditEventAsync(tokenId, tenantId, "Token metadata accessed", userId);
            
            // Save changes
            await _dbContext.SaveChangesAsync();
            
            return token;
        }

        public async Task<bool> DeleteTokenAsync(string tokenId, string tenantId, string userId = "system")
        {
            // Verify tenant exists
            var dbTenant = await _tenantService.GetTenantByIdAsync(tenantId);
            if (dbTenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            }
            
            var token = await _dbContext.Tokens
                .FirstOrDefaultAsync(t => t.Id == tokenId && t.TenantId == tenantId);
                
            if (token == null)
            {
                return false;
            }

            // Remove the token
            _dbContext.Tokens.Remove(token);
            
            // Log the event
            await LogAuditEventAsync(tokenId, tenantId, "Token deleted", userId);
            
            // Save changes
            await _dbContext.SaveChangesAsync();
            
            return true;
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string tokenId, string tenantId, string userId = "system")
        {
            // Verify tenant exists
            var dbTenant = await _tenantService.GetTenantByIdAsync(tenantId);
            if (dbTenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            }
            
            var logs = await _dbContext.AuditLogs
                .Where(l => l.TokenId == tokenId && l.TenantId == tenantId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            
            // Log the event
            await LogAuditEventAsync(tokenId, tenantId, "Audit logs accessed", userId);
            
            // Save changes
            await _dbContext.SaveChangesAsync();
            
            return logs;
        }

        private async Task LogAuditEventAsync(string tokenId, string tenantId, string action, string userId)
        {
            var log = new AuditLog
            {
                TokenId = tokenId,
                TenantId = tenantId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            await _dbContext.AuditLogs.AddAsync(log);
        }

        private string EncryptData(string data, string tenantId, DbTenant dbTenant, out string keyId)
        {
            var encryptionSettings = dbTenant.GetEncryptionSettings();
            
            // In a real implementation, you would use the tenant's encryption settings
            // with proper key management for each compliance standard
            
            // Generate a simulated key ID including tenant reference
            keyId = $"{tenantId}-{Guid.NewGuid()}";
            
            // For a PoC, we'll just convert to base64
            // In production, use proper encryption like AES with keys stored securely per tenant
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes);
        }

        private string DecryptData(string encryptedData, string tenantId, DbTenant dbTenant, string keyId)
        {
            // In a real implementation, you would retrieve the tenant-specific key
            // based on keyId and use it for decryption
            
            // For the PoC, just decode the base64 string
            byte[] bytes = Convert.FromBase64String(encryptedData);
            return Encoding.UTF8.GetString(bytes);
        }

        private string GenerateFingerprint(string data)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(bytes);
        }
    }
} 