using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using TokenizationService.Data;
using TokenizationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure MySQL database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 35));
builder.Services.AddDbContext<TokenizationService.Data.TokenizationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Configure tenant settings from appsettings.json (will be migrated to database on first run)
builder.Services.Configure<TokenizationService.Models.TenantSettings>(
    builder.Configuration.GetSection("TenantSettings"));

// Register tenant service
builder.Services.AddScoped<ITenantService, TenantService>();

// Register the tokenization service
builder.Services.AddScoped<TokenizationService.Services.ITokenizationService, TokenizationService.Services.TokenizationService>();
builder.Services.AddScoped<TenantInitializer>();

// Add Swagger with API key authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Tokenization Service API", Version = "v1" });
    
    // Define the API key security scheme
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key authentication",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    
    // Add the security requirement
    var key = new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
    {
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    };
    var requirement = new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { key, new List<string>() }
    };
    c.AddSecurityRequirement(requirement);
});

// Add API Key Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "ApiKeyScheme";
    options.DefaultChallengeScheme = "ApiKeyScheme";
})
.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TokenizationService.Authentication.ApiKeyAuthenticationHandler>("ApiKeyScheme", options => { });

builder.Services.AddAuthorization();

var app = builder.Build();

// Initialize database and tenants
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TokenizationService.Data.TokenizationDbContext>();
    var tenantInitializer = scope.ServiceProvider.GetRequiredService<TenantInitializer>();
    
    // Ensure database is created with the correct schema
    dbContext.Database.EnsureCreated();
    
    // Initialize tenant data
    try 
    {
        tenantInitializer.InitializeAsync().Wait();
        Console.WriteLine("Database initialized with tenant data");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
