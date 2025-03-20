using System;
using System.IO;

namespace TokenizationService
{
    class CreateTenantProgram
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: dotnet run --project CreateTenant.csproj <tenantId> <tenantName> <apiKey>");
                return;
            }

            string tenantId = args[0];
            string tenantName = args[1];
            string apiKey = args[2];
            
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Error: Configuration file not found at {configPath}");
                return;
            }
            
            try
            {
                TenantManagement.CreateTenant(configPath, tenantId, tenantName, apiKey);
                Console.WriteLine("Tenant created. Listing all tenants:");
                TenantManagement.ListTenants(configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tenant: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
} 