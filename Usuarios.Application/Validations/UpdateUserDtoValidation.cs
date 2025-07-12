using FluentValidation;

using Usuarios.Application.DTOs;

namespace Usuarios.Application.Validations
{
    public class UpdateUserDtoValidation : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidation()
        {
            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("El nombre no debe superar los 50 caracteres.");

            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("El apellido no debe superar los 50 caracteres.");

            RuleFor(x => x.Address)
                .MaximumLength(100).WithMessage("La dirección no debe superar los 100 caracteres.");

            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9\s\-()]*$").WithMessage("El teléfono debe contener solo números, espacios y caracteres especiales como +, -, (, ).")
                .MaximumLength(20).WithMessage("El teléfono no debe superar los 20 caracteres.");
        }
    }
}
