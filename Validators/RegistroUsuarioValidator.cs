using FluentValidation;

namespace ParkSmart;

public class RegistroUsuarioValidator : AbstractValidator<RegistroUsuarioDTO>
{
public RegistroUsuarioValidator()
    {
        RuleFor(u => u.nombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es obligatorio")
            .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(u => u.email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("Debe ser un email válido")
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(u => u.password)
            .NotEmpty().WithMessage("La contraseña es obligatoria")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(50).WithMessage("La contraseña no puede exceder 50 caracteres");

    }
}
