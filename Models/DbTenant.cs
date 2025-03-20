using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TokenizationService.Models
{
    public class DbTenant
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string ApiKey { get; set; } = string.Empty;
        
        // Store encryption settings as JSON
        public string? EncryptionSettingsJson { get; set; }
        
        // Store compliance options as JSON
        public string? ComplianceOptionsJson { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastModifiedAt { get; set; }
        
        public string CreatedBy { get; set; } = "system";
        
        public string? LastModifiedBy { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Admin flag for system administrators
        public bool IsAdmin { get; set; } = false;
        
        // Helper methods to convert between JSON and objects
        public EncryptionSettings GetEncryptionSettings()
        {
            try
            {
                if (string.IsNullOrEmpty(EncryptionSettingsJson))
                {
                    return new EncryptionSettings();
                }
                
                return JsonSerializer.Deserialize<EncryptionSettings>(EncryptionSettingsJson) 
                    ?? new EncryptionSettings();
            }
            catch (Exception)
            {
                // If there's any error, return default settings
                return new EncryptionSettings();
            }
        }
        
        public void SetEncryptionSettings(EncryptionSettings settings)
        {
            settings ??= new EncryptionSettings();
            EncryptionSettingsJson = JsonSerializer.Serialize(settings);
        }
        
        public ComplianceOptions GetComplianceOptions()
        {
            try
            {
                if (string.IsNullOrEmpty(ComplianceOptionsJson))
                {
                    return new ComplianceOptions();
                }
                
                return JsonSerializer.Deserialize<ComplianceOptions>(ComplianceOptionsJson) 
                    ?? new ComplianceOptions();
            }
            catch (Exception)
            {
                // If there's any error, return default options
                return new ComplianceOptions();
            }
        }
        
        public void SetComplianceOptions(ComplianceOptions options)
        {
            options ??= new ComplianceOptions();
            ComplianceOptionsJson = JsonSerializer.Serialize(options);
        }
        
        // Conversion to Tenant object for compatibility with existing code
        public Tenant ToTenant()
        {
            return new Tenant
            {
                Id = Id,
                Name = Name,
                ApiKey = ApiKey,
                EncryptionSettings = GetEncryptionSettings(),
                DefaultComplianceOptions = GetComplianceOptions(),
                IsAdmin = IsAdmin
            };
        }
    }
} 