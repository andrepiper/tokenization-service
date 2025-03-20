-- Use the tokenization database
USE tokenization;

-- Check if the IsAdmin column exists in the Tenants table
SET @columnExists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'tokenization' 
    AND TABLE_NAME = 'Tenants' 
    AND COLUMN_NAME = 'IsAdmin'
);

-- Add the IsAdmin column if it doesn't exist
SET @SQL = IF(@columnExists = 0, 
    'ALTER TABLE Tenants ADD COLUMN IsAdmin BOOLEAN NOT NULL DEFAULT FALSE',
    'SELECT "IsAdmin column already exists" AS Message');

PREPARE stmt FROM @SQL;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if the admin tenant already exists
SET @adminExists = (SELECT COUNT(*) FROM Tenants WHERE Id = 'admin-tenant');

-- If the admin tenant doesn't exist, create it
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
    
    SELECT 'Admin tenant created successfully.' AS Message;
ELSE
    -- If the admin tenant exists but doesn't have admin rights, update it
    UPDATE Tenants SET IsAdmin = TRUE WHERE Id = 'admin-tenant' AND IsAdmin = FALSE;
    SELECT 'Admin tenant already exists, ensured it has admin privileges.' AS Message;
END IF;

-- Show the current admin tenants
SELECT Id, Name, ApiKey, IsAdmin FROM Tenants WHERE IsAdmin = TRUE; 