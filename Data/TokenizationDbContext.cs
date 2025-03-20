using Microsoft.EntityFrameworkCore;
using TokenizationService.Models;
using System.Text.Json;

namespace TokenizationService.Data
{
    public class TokenizationDbContext : DbContext
    {
        public TokenizationDbContext(DbContextOptions<TokenizationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Token> Tokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DbTenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure tokens table
            modelBuilder.Entity<Token>()
                .HasKey(t => t.Id);
            
            modelBuilder.Entity<Token>()
                .HasIndex(t => new { t.TenantId, t.Id })
                .IsUnique();
            
            // Configure audit logs table
            modelBuilder.Entity<AuditLog>()
                .HasKey(a => a.Id);
            
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.TenantId, a.TokenId });
                
            // Configure tenants table
            modelBuilder.Entity<DbTenant>()
                .HasKey(t => t.Id);
            
            modelBuilder.Entity<DbTenant>()
                .HasIndex(t => t.ApiKey)
                .IsUnique();
        }
    }
    
    // Extension methods for Token to handle metadata serialization
    public static class TokenExtensions
    {
        public static void SerializeMetadata(this Token token)
        {
            try
            {
                if (token.Metadata != null)
                {
                    token.MetadataJson = JsonSerializer.Serialize(token.Metadata);
                }
                else
                {
                    token.MetadataJson = "{}";
                }
            }
            catch
            {
                // If serialization fails, default to empty JSON object
                token.MetadataJson = "{}";
            }
        }

        public static void DeserializeMetadata(this Token token)
        {
            try
            {
                if (token.MetadataJson != null && !object.ReferenceEquals(token.MetadataJson, DBNull.Value) 
                    && !string.IsNullOrEmpty(token.MetadataJson?.ToString()))
                {
                    token.Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(token.MetadataJson) 
                        ?? new Dictionary<string, string>();
                }
                else
                {
                    token.Metadata = new Dictionary<string, string>();
                }
            }
            catch
            {
                // If deserialization fails, use empty dictionary
                token.Metadata = new Dictionary<string, string>();
            }
        }
    }
} 