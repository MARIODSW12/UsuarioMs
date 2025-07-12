using FluentValidation;

using Usuarios.Application.DTOs;

namespace Usuarios.Application.Validations
{
    public class CreateUserActivityDtoValidation : AbstractValidator<CreateUserActivityDto>
    {
        public CreateUserActivityDtoValidation()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El UserId es obligatorio.")
                .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("El UserId debe ser un GUID válido.");

            RuleFor(x => x.Action)
                .NotEmpty().WithMessage("La acción es obligatoria.")
                .MaximumLength(100).WithMessage("La acción no debe superar los 100 caracteres.");
        }
    }
}
