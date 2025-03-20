using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TokenizationService.Models;
using TokenizationService.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace TokenizationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]  // Require Admin role for all tenant management operations
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DbTenant>>> GetTenants()
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            return Ok(tenants);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DbTenant>> GetTenant(string id)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }
            return Ok(tenant);
        }

        [HttpPost]
        public async Task<ActionResult<DbTenant>> CreateTenant(TenantCreateRequest request)
        {
            try
            {
                var dbTenant = new DbTenant
                {
                    Id = request.Id,
                    Name = request.Name,
                    ApiKey = request.ApiKey,
                    IsAdmin = request.IsAdmin
                };
                
                // Set encryption settings
                var encryptionSettings = new EncryptionSettings
                {
                    Algorithm = request.EncryptionAlgorithm ?? "AES-256",
                    KeyRotationPolicy = request.KeyRotationPolicy ?? "90Days",
                    MasterKeyReference = request.MasterKeyReference ?? $"{request.Id}-master-key"
                };
                dbTenant.SetEncryptionSettings(encryptionSettings);
                
                // Set compliance options
                var complianceOptions = new ComplianceOptions
                {
                    EnablePciCompliance = request.EnablePciCompliance,
                    EnableHipaaCompliance = request.EnableHipaaCompliance,
                    EnableSoc2Compliance = request.EnableSoc2Compliance,
                    EnableIso27001Compliance = request.EnableIso27001Compliance
                };
                dbTenant.SetComplianceOptions(complianceOptions);
                
                // Create tenant
                var createdTenant = await _tenantService.CreateTenantAsync(dbTenant);
                
                return CreatedAtAction(nameof(GetTenant), new { id = createdTenant.Id }, createdTenant);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DbTenant>> UpdateTenant(string id, TenantUpdateRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID in URL must match ID in request body");
            }
            
            try
            {
                var existingTenant = await _tenantService.GetTenantByIdAsync(id);
                if (existingTenant == null)
                {
                    return NotFound();
                }
                
                // Update basic properties
                existingTenant.Name = request.Name;
                existingTenant.ApiKey = request.ApiKey;
                existingTenant.LastModifiedBy = User.Identity?.Name ?? "system";
                
                // Update IsAdmin if provided
                if (request.IsAdmin.HasValue)
                {
                    existingTenant.IsAdmin = request.IsAdmin.Value;
                }
                
                // Update encryption settings if provided
                if (request.EncryptionAlgorithm != null || request.KeyRotationPolicy != null || request.MasterKeyReference != null)
                {
                    var settings = existingTenant.GetEncryptionSettings();
                    if (request.EncryptionAlgorithm != null)
                        settings.Algorithm = request.EncryptionAlgorithm;
                    if (request.KeyRotationPolicy != null)
                        settings.KeyRotationPolicy = request.KeyRotationPolicy;
                    if (request.MasterKeyReference != null)
                        settings.MasterKeyReference = request.MasterKeyReference;
                    
                    existingTenant.SetEncryptionSettings(settings);
                }
                
                // Update compliance options if any provided
                if (request.EnablePciCompliance != null || request.EnableHipaaCompliance != null ||
                    request.EnableSoc2Compliance != null || request.EnableIso27001Compliance != null)
                {
                    var options = existingTenant.GetComplianceOptions();
                    if (request.EnablePciCompliance != null)
                        options.EnablePciCompliance = request.EnablePciCompliance.Value;
                    if (request.EnableHipaaCompliance != null)
                        options.EnableHipaaCompliance = request.EnableHipaaCompliance.Value;
                    if (request.EnableSoc2Compliance != null)
                        options.EnableSoc2Compliance = request.EnableSoc2Compliance.Value;
                    if (request.EnableIso27001Compliance != null)
                        options.EnableIso27001Compliance = request.EnableIso27001Compliance.Value;
                    
                    existingTenant.SetComplianceOptions(options);
                }
                
                // Update tenant
                var updatedTenant = await _tenantService.UpdateTenantAsync(existingTenant);
                
                return Ok(updatedTenant);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTenant(string id)
        {
            var result = await _tenantService.DeleteTenantAsync(id);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
    
    public class TenantCreateRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string KeyRotationPolicy { get; set; }
        public string MasterKeyReference { get; set; }
        public bool EnablePciCompliance { get; set; } = true;
        public bool EnableHipaaCompliance { get; set; } = false;
        public bool EnableSoc2Compliance { get; set; } = true;
        public bool EnableIso27001Compliance { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
    }
    
    public class TenantUpdateRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string KeyRotationPolicy { get; set; }
        public string MasterKeyReference { get; set; }
        public bool? EnablePciCompliance { get; set; }
        public bool? EnableHipaaCompliance { get; set; }
        public bool? EnableSoc2Compliance { get; set; }
        public bool? EnableIso27001Compliance { get; set; }
        public bool? IsAdmin { get; set; }
    }
} 