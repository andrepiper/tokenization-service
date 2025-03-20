using System.Collections.Generic;

namespace TokenizationService.Models
{
    public class TenantSettings
    {
        public List<Tenant> Tenants { get; set; } = new List<Tenant>();
    }

    public class Tenant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public EncryptionSettings EncryptionSettings { get; set; } = new EncryptionSettings();
        public ComplianceOptions DefaultComplianceOptions { get; set; } = new ComplianceOptions();
        public bool IsAdmin { get; set; } = false;
    }

    public class EncryptionSettings
    {
        public string Algorithm { get; set; } = "AES-256";
        public string KeyRotationPolicy { get; set; } = "90Days";
        public string MasterKeyReference { get; set; } = "default";
    }
} 