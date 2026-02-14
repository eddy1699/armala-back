using Armala.Auth.Application.Commands;
using Armala.Auth.Application.DTOs;
using Armala.Auth.Domain.Repositories;
using Armala.Auth.Domain.ValueObjects;
using Armala.Auth.Infrastructure.Persistence.Entities;
using Armala.Auth.Infrastructure.Security;
using MediatR;
using PhoneNumber = Armala.Auth.Domain.ValueObjects.PhoneNumber;
using Email = Armala.Auth.Domain.ValueObjects.Email;
using Dni = Armala.Auth.Domain.ValueObjects.Dni;

namespace Armala.Armala.Auth.Application.CommandHandlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(
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

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Validar que el email no exista
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException("El email ya está registrado");

        // Validar que el teléfono no exista
        if (await _userRepository.ExistsByPhoneNumberAsync(request.PhoneNumber))
            throw new InvalidOperationException("El número de teléfono ya está registrado");

        // Validar que el DNI no exista
        if (await _userRepository.ExistsByDniAsync(request.Dni))
            throw new InvalidOperationException("El DNI ya está registrado");

        // Validar Value Objects
        var email = Email.Create(request.Email);
        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);
        var dni = Dni.Create(request.Dni);

        // Crear usuario
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email.Value,
            PhoneNumber = phoneNumber.Value,
            Dni = dni.Value,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Status = "NEW_USER",
            IsVerifired = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        // Generar tokens
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
