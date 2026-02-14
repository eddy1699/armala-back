using MediatR;
using Armala.Auth.Application.DTOs;

namespace Armala.Auth.Application.Commands;

public record LoginCommand : IRequest<AuthResponseDto>
{
    public string EmailOrPhone { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
