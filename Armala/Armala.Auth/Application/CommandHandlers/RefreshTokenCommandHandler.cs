using Armala.Auth.Application.Commands;
using Armala.Auth.Application.DTOs;
using Armala.Auth.Domain.Repositories;
using Armala.Auth.Infrastructure.Persistence.Entities;
using Armala.Auth.Infrastructure.Security;
using MediatR;

namespace Armala.Armala.Auth.Application.CommandHandlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (refreshToken == null)
            throw new UnauthorizedAccessException("Refresh token inválido");

        if (refreshToken.IsRevoked == true)
            throw new UnauthorizedAccessException("Refresh token revocado");

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expirado");

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId);

        if (user == null)
            throw new UnauthorizedAccessException("Usuario no encontrado");

        // Revocar el refresh token usado
        refreshToken.IsRevoked = true;
        refreshToken.UpdatedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        // Generar nuevos tokens
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.IsVerifired ?? false
        );

        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenExpiration = DateTime.UtcNow.AddDays(30);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = newRefreshTokenExpiration,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = _jwtTokenGenerator.GetTokenExpiration(newAccessToken) ?? DateTime.UtcNow.AddHours(2),
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
