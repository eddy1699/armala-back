namespace Armala.Auth.Application.DTOs;

public record AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public UserDto User { get; init; } = null!;
}

public record UserDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? ProfilePictureUrl { get; init; }
    public bool IsVerified { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}

public record LoginRequestDto
{
    public string EmailOrPhone { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record RegisterRequestDto
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Dni { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record SendOtpRequestDto
{
    public string Email { get; init; } = string.Empty;
}

public record VerifyOtpRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
