using System;
using System.Collections.Generic;

namespace TokenizationService.Models
{
    public class TokenizationRequest
    {
        public string Data { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public bool GenerateFingerprint { get; set; } = false;
        
        // Token expiration
        public DateTime? ExpiresAt { get; set; }
        
        // Compliance options
        public ComplianceOptions ComplianceOptions { get; set; } = new ComplianceOptions();
    }
} 