namespace TokenizationService.Models
{
    public class ComplianceOptions
    {
        public bool EnablePciCompliance { get; set; } = false;
        public bool EnableHipaaCompliance { get; set; } = false;
        public bool EnableSoc2Compliance { get; set; } = false;
        public bool EnableIso27001Compliance { get; set; } = false;
    }
} 