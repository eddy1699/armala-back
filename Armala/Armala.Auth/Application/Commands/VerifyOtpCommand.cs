using MediatR;

namespace Armala.Auth.Application.Commands;

public record VerifyOtpCommand : IRequest<VerifyOtpResultDto>
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

public record VerifyOtpResultDto
{
    public bool IsVerified { get; init; }
    public string Message { get; init; } = string.Empty;
}
