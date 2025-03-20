# Tokenization Service

A proof-of-concept tokenization platform for securely handling and processing sensitive data with multi-tenant support and MySQL persistence.


https://localhost:7211/swagger/index.html

![image](https://github.com/user-attachments/assets/a9424792-0b7f-4b23-8450-21a4ab3587b9)

![image](https://github.com/user-attachments/assets/a5b1a2c7-880b-4b2f-b631-534b563b2cc3)


## Features

- Tokenize sensitive data
- Detokenize to retrieve original data
- Store and retrieve tokens
- Delete tokens
- Generate fingerprints from sensitive data
- **Multi-tenancy support**: Isolated data and settings per tenant
- **MySQL Database**: Persistent storage of tokens, audit logs, and tenant configurations
- **Robust Error Handling**: Comprehensive handling of database null values and authentication errors

## Security Features

- **Multi-Tenant Architecture**: Complete data isolation between tenants
- **Per-Tenant API Keys**: Unique authentication for each tenant
- **Tenant-Specific Encryption**: Customizable encryption settings per tenant
- **API Key Authentication**: Secure access using API keys
- **Swagger Integration**: API documentation with security controls
- **Audit Logging**: Comprehensive activity tracking
- **Flexible Expiration**: Configurable token lifetimes
- **Persistent Storage**: Durable token storage using MySQL
- **Role-Based Administration**: Separate admin and regular tenant privileges
- **Improved Error Handling**: Protection against database NULL values and enhanced authentication error handling

## Compliance Standards

This tokenization platform is designed to help organizations meet the following compliance standards:

### HIPAA Compliant
- Protects electronic Protected Health Information (ePHI)
- Secures patient data through tokenization
- Supports required audit logging capabilities
- Enables secure data sharing within healthcare environments

### PCI DSS Level 1
- Meets requirements for handling payment card data
- Reduces PCI scope through tokenization of card data
- Supports secure storage and transmission of payment information
- Designed for Level 1 merchants processing over 6 million transactions annually

### SOC 2 Type II
- Supports security, availability, and confidentiality trust principles
- Enables organizations to maintain data security controls
- Provides mechanisms for secure processing of sensitive information
- Assists with meeting third-party audit requirements

### ISO 27001
- Aligns with Information Security Management System (ISMS) standards
- Supports risk assessment and management processes
- Helps organizations implement proper security controls
- Enables continuous security improvement

## Multi-Tenant Architecture

The service is designed with multi-tenancy in mind:

- Each tenant has its own isolated token storage
- Per-tenant API keys for authentication
- Tenant-specific encryption settings
- Tenant-specific compliance options
- Complete data isolation between tenants

## Administrator Access

The system supports two types of tenants:

1. **Regular Tenants**: Can only access their own data and tokens
2. **Admin Tenants**: Have access to tenant management functions

Admin tenants are designated with the `IsAdmin` flag and can:

- View all tenants in the system
- Create new tenants
- Update existing tenants
- Delete tenants (soft delete)

Regular tenants cannot access the tenant management endpoints. Only tenants with admin privileges can manage the tenant configuration.

### Setting Up an Admin Tenant

Admin tenants are automatically created during the database initialization process:

1. **Database Initialization**: The system creates an admin tenant with the credentials specified in the `seedtenant.sql` script.
2. **Default Admin Credentials**: By default, the system creates an admin tenant with the following credentials:
   - ID: `admin-tenant`
   - API Key: `admin-api-key-secure`

You can use these credentials to authenticate with the system and manage other tenants.

```
X-API-Key: admin-api-key-secure
```

For security in production environments, you should change this default API key after initial setup.

## Tenant Management

Tenants are managed through the `/api/tenants` endpoints. These endpoints allow you to:

- Create new tenants
- Update existing tenants
- Delete tenants (soft delete)
- View tenant information

### Tenant Properties

Each tenant has the following properties:

- `id`: Unique identifier for the tenant
- `name`: Display name for the tenant
- `apiKey`: API key used for authentication
- `encryptionSettings`: Tenant-specific encryption configuration
  - Algorithm (e.g., AES-256)
  - Key rotation policy
  - Master key reference
- `complianceOptions`: Tenant-specific compliance settings
  - PCI DSS
  - HIPAA
  - SOC 2
  - ISO 27001
- `isAdmin`: Flag indicating whether this tenant has admin privileges

## Database Configuration

The service uses MySQL for persistent storage of tokens, audit logs, and tenant configurations. Configure the connection in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=tokenization;User=root;Password=;"
}
```

The database schema is automatically created on first run, with tables for:
- `Tenants`: Stores all tenant configurations
- `Tokens`: Stores all tokenized data with tenant isolation
- `AuditLogs`: Records all operations on tokens

### Tenant Migration

On first run, any tenants configured in `appsettings.json` will be automatically migrated to the database. After migration, the application uses only the database for tenant configuration.

## API Documentation

The API is fully documented using Swagger UI, available at `/swagger` when running in development mode.

### Authentication

All API endpoints (except health checks) require authentication using a tenant-specific API key:

```
X-API-Key: your-tenant-api-key
```

### API Endpoints

#### Tokens

- `POST /api/tokens` - Create a token
- `GET /api/tokens/{id}` - Get token details
- `POST /api/tokens/detokenize` - Retrieve original data
- `DELETE /api/tokens/{id}` - Delete a token
- `GET /api/tokens/{id}/audit-logs` - Get audit logs for a token

#### Tenants

- `GET /api/tenants` - List all tenants (Admin only)
- `GET /api/tenants/{id}` - Get tenant details (Admin only)
- `POST /api/tenants` - Create a new tenant (Admin only)
- `PUT /api/tenants/{id}` - Update a tenant (Admin only)
- `DELETE /api/tenants/{id}` - Delete a tenant (Admin only)

#### Health Check

- `GET /api/health` - Check service health (no authentication required)

## Security Notes

This is a proof-of-concept implementation. For production use, you should implement the following security measures to meet compliance requirements:

### Encryption & Key Management
- Use AES-256 encryption for all sensitive data
- Implement key rotation policies as required by compliance standards
- Store encryption keys in a secure Hardware Security Module (HSM)
- Separate encryption keys for different data classifications and tenants

### Access Controls & Authentication
- Implement OAuth 2.0 or OpenID Connect for API authentication
- Role-based access control (RBAC) for all token operations
- Multi-factor authentication for administrative access
- IP address restrictions for API access

### Audit & Monitoring
- Comprehensive audit logging of all token operations
- Real-time monitoring of suspicious activities
- Alert mechanisms for unusual access patterns
- Log retention periods that comply with regulatory requirements

### Network Security
- TLS 1.3 for all API communications
- API rate limiting to prevent abuse
- Web Application Firewall (WAF) protection
- Network segmentation for tokenization services

### Data Storage
- Encrypted database for token storage
- Data compartmentalization based on sensitivity level
- Secure backup and recovery procedures
- Automatic data purging after retention period expiration

### API Key Management
- Store API keys securely with encryption
- Implement key rotation policies
- Allow for immediate key revocation
- Apply principle of least privilege with key permissions

## Recent Updates

### Improved Error Handling (v1.1)

- **Database NULL Values**: The system now handles database NULL values gracefully, ensuring that string properties on the `DbTenant` model are properly nullable and defaulted where appropriate. This prevents `InvalidCastException` errors when retrieving tenant information.

- **Authentication Error Handling**: The API Key authentication now provides detailed error messages and has improved exception handling to handle various failure scenarios:
  - Missing API keys
  - Empty API keys
  - Invalid API keys
  - Database type conversion errors

- **Default Values**: Proper default values are now set for required string properties to prevent null reference exceptions.

### Database Considerations

When setting up the database, ensure that your column types match the expected C# types. The system is now robust against:
- `DBNull` values from the database
- NULL string values in API keys and property fields
- Conversion errors between database types and .NET types

## Getting Started

1. Clone the repository
2. Configure MySQL connection in `appsettings.json`
3. Run the application: `dotnet run`
4. API will be available at `https://localhost:7xxx/api/tokens`
5. Swagger documentation available at `https://localhost:7xxx/swagger`
6. Manage tenants through the `/api/tenants` endpoints

## Source Control Best Practices

The project includes a `.gitignore` file configured for .NET projects. This file ensures that build artifacts, user-specific files, and sensitive information are not committed to source control.

### Files and Directories Not to Commit

The following files and directories are automatically excluded from Git:

- `bin/` and `obj/` directories (build artifacts)
- `.vs/` directory (Visual Studio cache)
- `*.user` files (user-specific settings)
- `appsettings.Development.json` (development configuration that may contain sensitive info)
- NuGet package directories

### Before Committing Changes

Before committing your changes to source control, ensure:

1. No sensitive information (like API keys or passwords) is hardcoded in the committed files
2. Development-specific settings are kept in `appsettings.Development.json`
3. All build artifacts have been cleaned (`dotnet clean` or manually removing `bin` and `obj` folders)

### Database Initialization Scripts

The database initialization scripts (`init.sql`, `seedtenant.sql`) are committed to source control as they are essential for setting up the application. However, in a production environment, you should:

1. Change default credentials in these scripts
2. Consider using a more secure method for managing database initialization
3. Follow your organization's security policies for managing database credentials

## Database Initialization and Seeding

The Tokenization Service uses a combination of Entity Framework Core and SQL scripts to initialize and seed the database with initial data.

### How Database Seeding Works

1. **Initial Setup**:
   - The system uses scripts in the project (`init.sql` and `seedtenant.sql`) to create the database schema and seed initial data
   - Database tables (Tenants, Tokens, AuditLogs) are created automatically if they don't exist

2. **Seeding Process Flow**:
   - When the application starts, the `TenantInitializer` class handles database initialization
   - It first checks if the database and required tables exist
   - If tables don't exist, they are created using Entity Framework Core
   - Then it checks for existing tenants in the database
   - Finally, it migrates any tenants defined in `appsettings.json` (if any) to the database

3. **Admin Tenant Creation**:
   - The `seedtenant.sql` script specifically looks for an admin tenant with ID "admin-tenant"
   - If not found, it automatically creates this admin tenant with:
     - ID: `admin-tenant`
     - Name: `System Administrator`
     - API Key: `admin-api-key-secure`
     - IsAdmin: `true`
   - This ensures there's always an admin account available for tenant management

4. **Execution Methods**:
   - **Automatic**: The seeding process runs automatically on application startup
   - **Manual**: You can manually execute the scripts using the provided PowerShell scripts:
     - `setup-mysql-tenant.ps1`: Sets up the database and initial tenant
     - `create-tenant.ps1`: Creates additional tenants as needed

5. **Tenant Migration**:
   - Any tenants defined in `appsettings.json` are migrated to the database on first run
   - After migration, the database becomes the source of truth for tenant information
   - This allows for backward compatibility with configuration-based tenant setup

This automated seeding ensures that the system is immediately usable without manual database setup, while still allowing for customization by modifying the seed scripts or adding tenants through the API once the system is running.
