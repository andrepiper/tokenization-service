-- Check if the tokenization database exists, create it if it doesn't
CREATE DATABASE IF NOT EXISTS tokenization;

-- Use the tokenization database
USE tokenization;

-- Create Tenants table if it doesn't exist
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

-- Create a unique index on ApiKey if it doesn't exist
CREATE UNIQUE INDEX IF NOT EXISTS IX_Tenants_ApiKey ON Tenants(ApiKey);

-- First, check if regular tenant exists
SET @tenantExists = (SELECT COUNT(*) FROM Tenants WHERE Id = 'db-tenant-1');

-- If it doesn't exist, add it
IF @tenantExists = 0 THEN
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
        'db-tenant-1', 
        'Database Tenant 1', 
        'tenant1-api-key', 
        '{"Algorithm":"AES-256","KeyRotationPolicy":"90Days","MasterKeyReference":"db-tenant-1-master-key"}', 
        '{"EnablePciCompliance":true,"EnableHipaaCompliance":false,"EnableSoc2Compliance":true,"EnableIso27001Compliance":true}', 
        NOW(), 
        'system', 
        TRUE,
        FALSE
    );
    
    SELECT 'Regular tenant created.' AS Message;
ELSE
    SELECT 'Regular tenant already exists.' AS Message;
END IF;

-- Next, check if admin tenant exists
SET @adminExists = (SELECT COUNT(*) FROM Tenants WHERE Id = 'admin-tenant');

-- If it doesn't exist, add it
IF @adminExists = 0 THEN
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
    );
    
    SELECT 'Admin tenant created.' AS Message;
ELSE
    SELECT 'Admin tenant already exists.' AS Message;
END IF;

-- Show all tenants
SELECT * FROM Tenants; 