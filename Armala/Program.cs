using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Armala.Auth.Infrastructure.Extensions;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURACION DE SERILOG
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/armala-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================================================
// CONFIGURACION DE CORS
// ============================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("ArmalaPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://armala.com",
                "https://www.armala.com",
                "https://armala-api-c8h7hvczchc4h8d2.scm.azurewebsites.net"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ============================================================================
// CONFIGURACION DE RATE LIMITING
// ============================================================================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ============================================================================
// CONFIGURACION DE JWT
// ============================================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    // Eventos de JWT para debugging y logging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("Autenticación JWT fallida: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var email = context.Principal?.FindFirst("email")?.Value ?? "unknown";
            Log.Information("Token JWT validado para usuario: {Email}", email);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Log.Warning("JWT Challenge: {Error} - {ErrorDescription}", 
                context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ============================================================================
// CONFIGURACION DE CONTROLLERS Y API
// ============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================================================================
// CONFIGURACION DE SWAGGER
// ============================================================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Armala API",
        Version = "v1",
        Description = "API del sistema Armala con autenticación JWT",
        Contact = new OpenApiContact
        {
            Name = "Equipo Armala",
            Email = "contacto@armala.com"
        }
    });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con el prefijo 'Bearer ' (ejemplo: Bearer eyJhbGc...)",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================================
// REGISTRO DE SERVICIOS DEL BOUNDED CONTEXT
// ============================================================================

// Infrastructure (DbContext, Repositories, Security)
builder.Services.AddAuthInfrastructure(builder.Configuration);

// MediatR (CQRS)
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// ============================================================================
// BUILD DE LA APLICACION
// ============================================================================
var app = builder.Build();

// ============================================================================
// CONFIGURACION DEL PIPELINE HTTP
// ============================================================================

// Exception Handling Middleware
app.UseMiddleware<Armala.Auth.Interfaces.Middleware.ExceptionHandlingMiddleware>();

// Swagger (solo en desarrollo y staging)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Armala API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Armala API Documentation";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.SupportedSubmitMethods(new[] { 
            SubmitMethod.Get, 
            SubmitMethod.Post, 
            SubmitMethod.Put, 
            SubmitMethod.Delete 
        });
    });
}

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    await next();
});

// Rate Limiting (debe ir antes de CORS)
app.UseIpRateLimiting();

// CORS
app.UseCors("ArmalaPolicy");

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Serilog Request Logging
app.UseSerilogRequestLogging();

// Controllers
app.MapControllers();

// ============================================================================
// HEALTH CHECK ENDPOINT
// ============================================================================
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "Armala API",
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithTags("Health")
.AllowAnonymous();

// ============================================================================
// ENDPOINT DE BIENVENIDA
// ============================================================================
app.MapGet("/", () => Results.Ok(new
{
    message = "Bienvenido a Armala API",
    documentation = "/swagger",
    health = "/health",
    version = "1.0.0"
}))
.WithName("Welcome")
.WithTags("Info")
.AllowAnonymous();

// ============================================================================
// INICIAR APLICACION
// ============================================================================
try
{
    Log.Information("Iniciando Armala API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion fallo al iniciar");
}
finally
{
    Log.CloseAndFlush();
}
