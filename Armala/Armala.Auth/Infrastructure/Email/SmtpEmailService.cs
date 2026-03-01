using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Armala.Auth.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string fullName, string otpCode)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var fromEmail = emailSettings["FromEmail"]!;
        var fromName = emailSettings["FromName"] ?? "Armala";
        var smtpHost = emailSettings["SmtpHost"]!;
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var password = emailSettings["Password"]!;

        var subject = "Código de verificación - Armala";
        var body = BuildOtpEmailBody(fullName, otpCode);

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(fromEmail, password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
        _logger.LogInformation("OTP email enviado a {Email}", toEmail);
    }

    private static string BuildOtpEmailBody(string fullName, string otpCode)
    {
        var year = DateTime.UtcNow.Year;
        return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"" />
  <style>
    body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
    .container {{ max-width: 500px; margin: 40px auto; background: #ffffff; border-radius: 8px; padding: 32px; }}
    .header {{ text-align: center; margin-bottom: 24px; }}
    .header h1 {{ color: #1a1a2e; font-size: 24px; margin: 0; }}
    .otp-box {{ background: #f0f4ff; border-radius: 8px; text-align: center; padding: 24px; margin: 24px 0; }}
    .otp-code {{ font-size: 40px; font-weight: bold; letter-spacing: 10px; color: #3b5bdb; }}
    .footer {{ text-align: center; color: #888; font-size: 12px; margin-top: 24px; }}
    p {{ color: #444; line-height: 1.6; }}
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""header"">
      <h1>Armala</h1>
    </div>
    <p>Hola <strong>{fullName}</strong>,</p>
    <p>Usa el siguiente código para verificar tu cuenta. Este código expira en <strong>5 minutos</strong>.</p>
    <div class=""otp-box"">
      <div class=""otp-code"">{otpCode}</div>
    </div>
    <p>Si no solicitaste este código, puedes ignorar este correo.</p>
    <div class=""footer"">
      <p>Armala &copy; {year}. Todos los derechos reservados.</p>
    </div>
  </div>
</body>
</html>";
    }
}
