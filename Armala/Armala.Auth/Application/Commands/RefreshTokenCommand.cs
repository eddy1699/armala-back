using MediatR;
using Armala.Auth.Application.DTOs;

namespace Armala.Auth.Application.Commands;

public record RefreshTokenCommand : IRequest<AuthResponseDto>
{
    public string RefreshToken { get; init; } = string.Empty;
}
