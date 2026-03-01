using MediatR;
using Microsoft.Extensions.Configuration;
using Armala.Auth.Application.Commands;
using Armala.Auth.Domain.Repositories;

namespace Armala.Auth.Application.CommandHandlers;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, VerifyOtpResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IConfiguration _configuration;

    public VerifyOtpCommandHandler(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _configuration = configuration;
    }

    public async Task<VerifyOtpResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("No existe una cuenta con ese correo electrónico");

        if (user.IsVerifired == true)
            throw new InvalidOperationException("La cuenta ya está verificada");

        var otp = await _otpRepository.GetActiveByUserIdAndPurposeAsync(user.Id, "EMAIL_VERIFICATION")
            ?? throw new InvalidOperationException("No hay un código activo. Por favor solicita uno nuevo.");

        var maxAttempts = int.Parse(_configuration["OtpSettings:MaxAttempts"] ?? "3");

        if (otp.Attempts >= maxAttempts)
        {
            otp.IsUsed = true;
            await _otpRepository.UpdateAsync(otp);
            throw new InvalidOperationException("Demasiados intentos fallidos. Solicita un nuevo código.");
        }

        if (otp.Code != request.Code)
        {
            otp.Attempts += 1;
            await _otpRepository.UpdateAsync(otp);

            var remaining = maxAttempts - otp.Attempts;
            throw new InvalidOperationException($"Código incorrecto. Te quedan {remaining} intento(s).");
        }

        // Código correcto: marcar como usado y verificar al usuario
        otp.IsUsed = true;
        await _otpRepository.UpdateAsync(otp);

        user.IsVerifired = true;
        user.Status = "ACTIVE";
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return new VerifyOtpResultDto
        {
            IsVerified = true,
            Message = "¡Cuenta verificada exitosamente!"
        };
    }
}
