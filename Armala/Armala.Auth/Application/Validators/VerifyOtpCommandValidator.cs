using FluentValidation;
using Armala.Auth.Application.Commands;

namespace Armala.Auth.Application.Validators;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no tiene un formato válido");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código OTP es requerido")
            .Length(6).WithMessage("El código OTP debe tener 6 dígitos")
            .Matches(@"^\d{6}$").WithMessage("El código OTP solo debe contener dígitos");
    }
}
