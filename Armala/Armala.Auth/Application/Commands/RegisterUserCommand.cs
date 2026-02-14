using MediatR;
using Armala.Auth.Application.DTOs;

namespace Armala.Auth.Application.Commands;

public record RegisterUserCommand : IRequest<AuthResponseDto>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Dni { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
