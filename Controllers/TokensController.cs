using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TokenizationService.Models;
using TokenizationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace TokenizationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TokensController : ControllerBase
    {
        private readonly ITokenizationService _tokenizationService;

        public TokensController(ITokenizationService tokenizationService)
        {
            _tokenizationService = tokenizationService;
        }

        [HttpPost]
        public async Task<ActionResult<Token>> CreateToken(TokenizationRequest request)
        {
            // Get the tenant ID and user ID from the authenticated user
            var tenantId = User.Identity?.Name ?? "unknown";
            var userId = User.Identity?.Name ?? "anonymous";
            
            var token = await _tokenizationService.TokenizeAsync(request, tenantId, userId);
            
            return CreatedAtAction(nameof(GetToken), new { id = token.Id }, token);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Token>> GetToken(string id)
        {
            try
            {
                var tenantId = User.Identity?.Name ?? "unknown";
                var userId = User.Identity?.Name ?? "anonymous";
                
                var token = await _tokenizationService.GetTokenAsync(id, tenantId, userId);
                return Ok(token);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("detokenize")]
        public async Task<ActionResult<string>> Detokenize(DetokenizationRequest request)
        {
            try
            {
                var tenantId = User.Identity?.Name ?? "unknown";
                var userId = User.Identity?.Name ?? "anonymous";
                
                var data = await _tokenizationService.DetokenizeAsync(request.TokenId, tenantId, userId);
                return Ok(new { data });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteToken(string id)
        {
            try
            {
                var tenantId = User.Identity?.Name ?? "unknown";
                var userId = User.Identity?.Name ?? "anonymous";
                
                var result = await _tokenizationService.DeleteTokenAsync(id, tenantId, userId);
                if (result)
                {
                    return NoContent();
                }
                return NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/audit-logs")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs(string id)
        {
            try
            {
                var tenantId = User.Identity?.Name ?? "unknown";
                var userId = User.Identity?.Name ?? "anonymous";
                
                // First check if the token exists
                await _tokenizationService.GetTokenAsync(id, tenantId, userId);
                var logs = await _tokenizationService.GetAuditLogsAsync(id, tenantId, userId);
                return Ok(logs);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
} 