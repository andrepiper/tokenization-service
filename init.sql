-- =============================================
-- Tokenization Service Database Initialization
-- =============================================

-- Create the database if it doesn't exist
CREATE DATABASE IF NOT EXISTS tokenization;

-- Use the tokenization database
USE tokenization;

-- =============================================
-- Create Tables
-- =============================================

-- Create Tenants table
CREATE TABLE IF NOT EXISTS Tenants (
    Id VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    ApiKey VARCHAR(100) NOT NULL,
    EncryptionSettingsJson TEXT,
    ComplianceOptionsJson TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastModifiedAt DATETIME NULL,
    CreatedBy VARCHAR(50) NOT NULL DEFAULT 'system',
    LastModifiedBy VARCHAR(50) NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    IsAdmin BOOLEAN NOT NULL DEFAULT FALSE
);

-- Create Tokens table
CREATE TABLE IF NOT EXISTS Tokens (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    TenantId VARCHAR(50) NOT NULL,
    Data TEXT NOT NULL,
    Type VARCHAR(50) NOT NULL,
    MetadataJson TEXT NULL,
    Fingerprint VARCHAR(255) NULL,
    EncryptionKeyId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NULL,
    LastAccessedAt DATETIME NULL,
    LastAccessedBy VARCHAR(50) NULL,
    IsPci BOOLEAN NOT NULL DEFAULT FALSE,
    IsHipaa BOOLEAN NOT NULL DEFAULT FALSE,
    IsSoc2 BOOLEAN NOT NULL DEFAULT FALSE,
    IsIso27001 BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

-- Create AuditLogs table
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    TokenId CHAR(36) NOT NULL,
    TenantId VARCHAR(50) NOT NULL,
    Action VARCHAR(100) NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UserId VARCHAR(50) NOT NULL,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

-- =============================================
-- Create Indexes
-- =============================================

-- Create index on ApiKey for Tenants
CREATE UNIQUE INDEX IF NOT EXISTS IX_Tenants_ApiKey ON Tenants(ApiKey);

-- Create compound index for Tokens (TenantId, Id)
CREATE UNIQUE INDEX IF NOT EXISTS IX_Tokens_TenantId_Id ON Tokens(TenantId, Id);

-- Create index for AuditLogs (TenantId, TokenId)
CREATE INDEX IF NOT EXISTS IX_AuditLogs_TenantId_TokenId ON AuditLogs(TenantId, TokenId);

-- =============================================
-- Seed Initial Data
-- =============================================

-- Seed Regular Tenant
INSERT INTO Tenants (
    Id, 
    Name, 
    ApiKey, 
    EncryptionSettingsJson, 
    ComplianceOptionsJson, 
    CreatedAt, 
    CreatedBy, 
    IsActive,
    IsAdmin
) VALUES (
    'tenant1', 
    'Regular Tenant', 
    'tenant1-api-key', 
    '{"Algorithm":"AES-256","KeyRotationPolicy":"90Days","MasterKeyReference":"tenant1-master-key"}', 
    '{"EnablePciCompliance":true,"EnableHipaaCompliance":false,"EnableSoc2Compliance":true,"EnableIso27001Compliance":true}', 
    NOW(), 
    'system', 
    TRUE,
    FALSE
) ON DUPLICATE KEY UPDATE 
    Name = VALUES(Name),
    ApiKey = VALUES(ApiKey),
    EncryptionSettingsJson = VALUES(EncryptionSettingsJson),
    ComplianceOptionsJson = VALUES(ComplianceOptionsJson),
    LastModifiedAt = NOW();

-- Seed Admin Tenant
INSERT INTO Tenants (
    Id, 
    Name, 
    ApiKey, 
    EncryptionSettingsJson, 
    ComplianceOptionsJson, 
    CreatedAt, 
    CreatedBy, 
    IsActive,
    IsAdmin
) VALUES (
    'admin-tenant', 
    'System Administrator', 
    'admin-api-key-secure', 
    '{"Algorithm":"AES-256","KeyRotationPolicy":"30Days","MasterKeyReference":"admin-master-key"}', 
    '{"EnablePciCompliance":true,"EnableHipaaCompliance":true,"EnableSoc2Compliance":true,"EnableIso27001Compliance":true}', 
    NOW(), 
    'system', 
    TRUE,
    TRUE
) ON DUPLICATE KEY UPDATE 
    Name = VALUES(Name),
    ApiKey = VALUES(ApiKey),
    EncryptionSettingsJson = VALUES(EncryptionSettingsJson),
    ComplianceOptionsJson = VALUES(ComplianceOptionsJson),
    IsAdmin = VALUES(IsAdmin),
    LastModifiedAt = NOW();

-- =============================================
-- Sample Tokens (Optional)
-- =============================================

-- Create a sample token for tenant1
INSERT INTO Tokens (
    Id,
    TenantId,
    Data,
    Type,
    MetadataJson,
    EncryptionKeyId,
    CreatedAt,
    IsPci,
    IsHipaa,
    IsSoc2,
    IsIso27001
) VALUES (
    UUID(),
    'tenant1',
    'U2FtcGxlIGRhdGEgZm9yIHRlbmFudDogUmVndWxhciBUZW5hbnQ=',  -- Base64 for 'Sample data for tenant: Regular Tenant'
    'sample',
    '{"purpose":"initialization","environment":"development"}',
    'tenant1-sample-key',
    NOW(),
    TRUE,
    FALSE,
    TRUE,
    TRUE
) ON DUPLICATE KEY UPDATE
    Id = VALUES(Id);

-- Create a sample token for admin-tenant
INSERT INTO Tokens (
    Id,
    TenantId,
    Data,
    Type,
    MetadataJson,
    EncryptionKeyId,
    CreatedAt,
    IsPci,
    IsHipaa,
    IsSoc2,
    IsIso27001
) VALUES (
    UUID(),
    'admin-tenant',
    'U2FtcGxlIGRhdGEgZm9yIHRlbmFudDogU3lzdGVtIEFkbWluaXN0cmF0b3I=',  -- Base64 for 'Sample data for tenant: System Administrator'
    'sample',
    '{"purpose":"initialization","environment":"development"}',
    'admin-tenant-sample-key',
    NOW(),
    TRUE,
    TRUE,
    TRUE,
    TRUE
) ON DUPLICATE KEY UPDATE
    Id = VALUES(Id);

-- =============================================
-- Validate the database setup
-- =============================================

-- Show tables
SELECT 'Tables in tokenization database:' AS Message;
SHOW TABLES;

-- Display tenant information
SELECT 'Tenant Information:' AS Message;
SELECT Id, Name, ApiKey, IsAdmin, IsActive FROM Tenants;

-- Display all columns in the Tenants table to help diagnose casting issues
SELECT 'Tenant Columns:' AS Message;
SHOW COLUMNS FROM Tenants;

-- Display token information
SELECT 'Token Information:' AS Message;
SELECT Id, TenantId, Type, CreatedAt FROM Tokens; 