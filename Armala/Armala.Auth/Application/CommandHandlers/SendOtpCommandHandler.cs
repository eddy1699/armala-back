using MediatR;
using Microsoft.Extensions.Configuration;
using Armala.Auth.Application.Commands;
using Armala.Auth.Domain.Repositories;
using Armala.Auth.Infrastructure.Email;
using Armala.Auth.Infrastructure.Persistence.Entities;

namespace Armala.Auth.Application.CommandHandlers;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, SendOtpResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public SendOtpCommandHandler(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<SendOtpResultDto> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("No existe una cuenta con ese correo electrónico");

        if (user.IsVerifired == true)
            throw new InvalidOperationException("La cuenta ya está verificada");

        var otpSettings = _configuration.GetSection("OtpSettings");
        var expirationMinutes = int.Parse(otpSettings["ExpirationMinutes"] ?? "5");
        var cooldownSeconds = int.Parse(otpSettings["ResendCooldownSeconds"] ?? "60");

        // Verificar cooldown: si ya hay un OTP activo creado recientemente, no enviar otro
        var existingOtp = await _otpRepository.GetActiveByUserIdAndPurposeAsync(user.Id, "EMAIL_VERIFICATION");
        if (existingOtp != null)
        {
            var secondsSinceCreation = (DateTime.UtcNow - (existingOtp.CreatedAt ?? DateTime.UtcNow)).TotalSeconds;
            if (secondsSinceCreation < cooldownSeconds)
            {
                var remaining = cooldownSeconds - (int)secondsSinceCreation;
                return new SendOtpResultDto
                {
                    Message = $"Ya se envió un código recientemente. Espera {remaining} segundos para solicitar uno nuevo.",
                    CooldownSeconds = remaining
                };
            }
        }

        // Invalidar OTPs anteriores
        await _otpRepository.InvalidateAllByUserIdAndPurposeAsync(user.Id, "EMAIL_VERIFICATION");

        // Generar nuevo código
        var code = GenerateOtpCode(int.Parse(otpSettings["Length"] ?? "6"));

        var otp = new Otp
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Code = code,
            Purpose = "EMAIL_VERIFICATION",
            IsUsed = false,
            Attempts = 0,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            CreatedAt = DateTime.UtcNow
        };

        await _otpRepository.AddAsync(otp);

        await _emailService.SendOtpEmailAsync(user.Email, user.FullName, code);

        return new SendOtpResultDto
        {
            Message = $"Código de verificación enviado a {MaskEmail(user.Email)}",
            CooldownSeconds = cooldownSeconds
        };
    }

    private static string GenerateOtpCode(int length)
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10).ToString()));
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return email;
        var name = parts[0];
        var visible = name.Length > 2 ? name[..2] : name[..1];
        return $"{visible}***@{parts[1]}";
    }
}
