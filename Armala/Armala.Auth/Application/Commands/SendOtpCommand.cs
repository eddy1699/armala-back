using MediatR;

namespace Armala.Auth.Application.Commands;

public record SendOtpCommand : IRequest<SendOtpResultDto>
{
    public string Email { get; init; } = string.Empty;
}

public record SendOtpResultDto
{
    public string Message { get; init; } = string.Empty;
    public int CooldownSeconds { get; init; }
}
