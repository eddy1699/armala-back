using Armala.Armala.Auth.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Armala.Auth.Domain.Repositories;
using Armala.Auth.Infrastructure.Persistence;
using Armala.Auth.Infrastructure.Persistence.Repositories;
using Armala.Auth.Infrastructure.Security;
using Armala.Auth.Infrastructure.Email;

namespace Armala.Auth.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddArmalaDbContext(configuration);

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();

        // Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Email Service
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}
