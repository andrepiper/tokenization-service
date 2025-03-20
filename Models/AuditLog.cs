using System;
using System.ComponentModel.DataAnnotations;

namespace TokenizationService.Models
{
    public class AuditLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string TokenId { get; set; }
        
        [Required]
        public string TenantId { get; set; }
        
        [Required]
        public string Action { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        public string UserId { get; set; }
    }
} 