using FluentValidation;

namespace ParkSmart;

public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(l => l.email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("Debe ser un email válido");

        RuleFor(l => l.password)
            .NotEmpty().WithMessage("La contraseña es obligatoria");
    }
}
