using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TokenizationService.Models
{
    public class Token
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string TenantId { get; set; }
        
        [Required]
        public string Data { get; set; }
        
        public string Type { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        public string? Fingerprint { get; set; }
        
        [JsonIgnore]
        [NotMapped]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        
        // Serialized version of Metadata for database storage
        public string MetadataJson { get; set; } = "{}";
        
        // Compliance-related properties
        public bool IsPci { get; set; }
        public bool IsHipaa { get; set; }
        public bool IsSoc2 { get; set; }
        public bool IsIso27001 { get; set; }
        
        public string? EncryptionKeyId { get; set; }
        public string? LastAccessedBy { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }
} 