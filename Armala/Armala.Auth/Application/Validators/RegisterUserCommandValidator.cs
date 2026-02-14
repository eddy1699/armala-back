using FluentValidation;
using Armala.Auth.Application.Commands;

namespace Armala.Auth.Application.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(255).WithMessage("El email no puede exceder 255 caracteres");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("El teléfono es requerido")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Formato de teléfono inválido");

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es requerido")
            .Matches(@"^\d{8}$").WithMessage("El DNI debe tener 8 dígitos");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula")
            .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una minúscula")
            .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número")
            .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial");
    }
}
