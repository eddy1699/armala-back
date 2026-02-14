using MediatR;
using Armala.Auth.Application.Commands;
using Armala.Auth.Application.DTOs;
using Armala.Auth.Domain.Repositories;
using Armala.Auth.Infrastructure.Persistence.Entities;
using Armala.Auth.Infrastructure.Security;

namespace Armala.Auth.Application.CommandHandlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuario por email o teléfono
        User? user = null;

        if (request.EmailOrPhone.Contains("@"))
        {
            user = await _userRepository.GetByEmailAsync(request.EmailOrPhone);
        }
        else
        {
            user = await _userRepository.GetByPhoneNumberAsync(request.EmailOrPhone);
        }

        if (user == null)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        // Verificar si el usuario está suspendido
        if (user.Status == "SUSPENDED" || user.Status == "BANNED")
            throw new UnauthorizedAccessException("Cuenta suspendida o bloqueada");

        // Revocar tokens anteriores (opcional, para mayor seguridad)
        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        // Generar nuevos tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.IsVerifired ?? false
        );

        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(30);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = refreshTokenExpiration,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        // Actualizar última conexión
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = _jwtTokenGenerator.GetTokenExpiration(accessToken) ?? DateTime.UtcNow.AddHours(2),
            TokenType = "Bearer",
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsVerified = user.IsVerifired ?? false,
                Status = user.Status ?? "NEW_USER",
                CreatedAt = user.CreatedAt ?? DateTime.UtcNow
            }
        };
    }
}
