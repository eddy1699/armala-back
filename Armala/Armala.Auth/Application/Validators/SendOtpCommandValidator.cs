using FluentValidation;
using Armala.Auth.Application.Commands;

namespace Armala.Auth.Application.Validators;

public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no tiene un formato válido");
    }
}
