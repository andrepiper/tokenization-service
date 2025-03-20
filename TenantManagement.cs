using Microsoft.Extensions.Options;
using System.Text.Json;
using TokenizationService.Models;

namespace TokenizationService
{
    public class TenantManagement
    {
        public static void CreateTenant(string configPath, string tenantId, string tenantName, string apiKey)
        {
            // Read current appsettings.json
            var jsonString = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, options);
            
            // Get TenantSettings section
            var tenantSettingsJson = config["TenantSettings"].ToString();
            var tenantSettings = JsonSerializer.Deserialize<TenantSettings>(tenantSettingsJson, options);
            
            // Check if tenant already exists
            if (tenantSettings.Tenants.Any(t => t.Id == tenantId))
            {
                Console.WriteLine($"Tenant with ID {tenantId} already exists.");
                return;
            }
            
            // Create new tenant
            var newTenant = new Tenant
            {
                Id = tenantId,
                Name = tenantName,
                ApiKey = apiKey,
                EncryptionSettings = new EncryptionSettings
                {
                    Algorithm = "AES-256",
                    KeyRotationPolicy = "90Days",
                    MasterKeyReference = $"{tenantId}-master-key"
                },
                DefaultComplianceOptions = new ComplianceOptions
                {
                    EnablePciCompliance = true,
                    EnableHipaaCompliance = true,
                    EnableSoc2Compliance = true,
                    EnableIso27001Compliance = true
                }
            };
            
            // Add tenant to settings
            tenantSettings.Tenants.Add(newTenant);
            
            // Update config object
            config["TenantSettings"] = tenantSettings;
            
            // Write back to appsettings.json
            var updatedJsonString = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configPath, updatedJsonString);
            
            Console.WriteLine($"Tenant {tenantName} (ID: {tenantId}) created successfully with API key: {apiKey}");
        }
        
        public static void ListTenants(string configPath)
        {
            // Read current appsettings.json
            var jsonString = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, options);
            
            // Get TenantSettings section
            var tenantSettingsJson = config["TenantSettings"].ToString();
            var tenantSettings = JsonSerializer.Deserialize<TenantSettings>(tenantSettingsJson, options);
            
            // Display tenants
            Console.WriteLine("Configured Tenants:");
            Console.WriteLine("------------------");
            
            foreach (var tenant in tenantSettings.Tenants)
            {
                Console.WriteLine($"ID: {tenant.Id}");
                Console.WriteLine($"Name: {tenant.Name}");
                Console.WriteLine($"API Key: {tenant.ApiKey}");
                Console.WriteLine($"Encryption Algorithm: {tenant.EncryptionSettings.Algorithm}");
                Console.WriteLine($"Key Rotation Policy: {tenant.EncryptionSettings.KeyRotationPolicy}");
                Console.WriteLine("Default Compliance Settings:");
                Console.WriteLine($"  - PCI: {tenant.DefaultComplianceOptions.EnablePciCompliance}");
                Console.WriteLine($"  - HIPAA: {tenant.DefaultComplianceOptions.EnableHipaaCompliance}");
                Console.WriteLine($"  - SOC2: {tenant.DefaultComplianceOptions.EnableSoc2Compliance}");
                Console.WriteLine($"  - ISO27001: {tenant.DefaultComplianceOptions.EnableIso27001Compliance}");
                Console.WriteLine("------------------");
            }
        }
    }
} 