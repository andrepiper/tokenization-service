-- Create database if it doesn't exist
CREATE DATABASE IF NOT EXISTS tokenization;

USE tokenization;

-- Create sample token for new tenant
INSERT INTO Tokens (
    Id, 
    TenantId, 
    Data, 
    Type, 
    CreatedAt, 
    MetadataJson, 
    IsPci, 
    IsHipaa, 
    IsSoc2, 
    IsIso27001, 
    EncryptionKeyId
) VALUES (
    UUID(), 
    'tenant3', 
    'U2FtcGxlIGRhdGEgZm9yIHRlbmFudDogVGhpcmQgVGVuYW50', -- Base64 encoded "Sample data for tenant: Third Tenant"
    'sample',
    UTC_TIMESTAMP(),
    '{"purpose":"initialization","environment":"development"}',
    true,
    true,
    true,
    true,
    'tenant3-12345'
);

-- Get the token ID we just created
SET @token_id = (SELECT Id FROM Tokens WHERE TenantId = 'tenant3' ORDER BY CreatedAt DESC LIMIT 1);

-- Create audit log for the new token
INSERT INTO AuditLogs (
    Id,
    TokenId,
    TenantId,
    Action,
    Timestamp,
    UserId
) VALUES (
    UUID(),
    @token_id,
    'tenant3',
    'Tenant initialization',
    UTC_TIMESTAMP(),
    'system'
);

-- Show the new tenant data
SELECT t.Id, t.TenantId, t.Type, t.CreatedAt, a.Action, a.Timestamp
FROM Tokens t
JOIN AuditLogs a ON t.Id = a.TokenId
WHERE t.TenantId = 'tenant3'; 