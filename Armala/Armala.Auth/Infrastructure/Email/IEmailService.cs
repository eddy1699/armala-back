namespace Armala.Auth.Infrastructure.Email;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string fullName, string otpCode);
}
