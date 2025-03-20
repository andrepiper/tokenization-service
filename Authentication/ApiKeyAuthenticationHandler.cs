using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TokenizationService.Models;
using TokenizationService.Services;

namespace TokenizationService.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = "X-API-Key";
        private readonly ITenantService _tenantService;
        
#pragma warning disable CS0618 // Type or member is obsolete
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ITenantService tenantService) : base(options, logger, encoder, clock)
        {
            _tenantService = tenantService;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if the API key header exists
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.Fail("API Key is missing");
            }
            
            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.Fail("API Key is empty");
            }
            
            // Find the tenant with the provided API key
            var tenant = await _tenantService.GetTenantByApiKeyAsync(providedApiKey);
            
            if (tenant == null)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }
            
            // Create claims for the authenticated tenant
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tenant.Id),
                new Claim(ClaimTypes.Name, tenant.Id),
                new Claim("TenantName", tenant.Name)
            };
            
            // Add admin role claim if tenant is an admin
            if (tenant.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            
            return AuthenticateResult.Success(ticket);
        }
    }
} 