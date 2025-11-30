using FluentValidation;

namespace ParkSmart;

public class CrearSedeValidator : AbstractValidator<CrearSedeDTO>
{
public CrearSedeValidator()
    {
        RuleFor(s => s.nombre)
            .NotEmpty().WithMessage("El nombre de la sede es obligatorio")
            .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres")            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");
        
        RuleFor(s => s.direccion)
            .NotEmpty().WithMessage("La dirección es obligatoria")            .MinimumLength(10).WithMessage("La dirección debe tener al menos 10 caracteres");
        
        RuleFor(s => s.passwordAcceso)
            .NotEmpty().WithMessage("La contraseña de acceso es obligatoria")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres")            .MaximumLength(50).WithMessage("La contraseña no puede exceder 50 caracteres");
        
        RuleFor(s => s.tarifaPorHora)
            .GreaterThan(0).WithMessage("La tarifa por hora debe ser mayor a 0")            .LessThanOrEqualTo(9999.99m).WithMessage("La tarifa por hora no puede exceder 9999.99");
        
        RuleFor(s => s.multaPorHora)
            .GreaterThanOrEqualTo(0).WithMessage("La multa por hora no puede ser negativa")            .LessThanOrEqualTo(9999.99m).WithMessage("La multa por hora no puede exceder 9999.99");
        
        RuleFor(s => s.montoMaximoMulta)
            .GreaterThan(0).WithMessage("El monto máximo de multa debe ser mayor a 0")
            .When(s => s.multaConTope)            .WithName("Monto máximo de multa");
        
        RuleFor(s => s.niveles)
            .NotEmpty().WithMessage("Debe especificar al menos un nivel")            .Must(niveles => niveles.Count > 0).WithMessage("La sede debe tener al menos un nivel");
        
        RuleForEach(s => s.niveles).ChildRules(nivel =>
        {
            nivel.RuleFor(n => n.numeroPiso)                
                .NotEqual(0).WithMessage("El número de piso no puede ser 0");
            nivel.RuleFor(n => n.capacidad)
                .GreaterThan(0).WithMessage("La capacidad del nivel debe ser mayor a 0")
                .LessThanOrEqualTo(500).WithMessage("La capacidad del nivel no puede exceder 500 cajones");        
        });
        
        RuleFor(s => s.niveles)
            .Must(niveles => niveles.Select(n => n.numeroPiso).Distinct().Count() == niveles.Count)
            .WithMessage("No puede haber números de piso duplicados");
    }
}
