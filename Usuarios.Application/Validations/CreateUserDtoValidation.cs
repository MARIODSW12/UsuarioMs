using FluentValidation;

using Usuarios.Application.DTOs;

namespace Usuarios.Application.Validations
{

    public class CreateUserDtoValidation : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre no debe superar los 50 caracteres.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(50).WithMessage("El apellido no debe superar los 50 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("Debe ingresar un email válido.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("El ID del rol es obligatorio.")
                .Matches("^[a-fA-F0-9]{24}$").WithMessage("El ID del rol debe ser un ObjectId válido.");

            RuleFor(x => x.Address)
                .MaximumLength(100).WithMessage("La dirección no debe superar los 100 caracteres.");

            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9\s\-()]*$").WithMessage("El teléfono debe contener solo números, espacios y caracteres especiales como +, -, (, ).")
                .MaximumLength(20).WithMessage("El teléfono no debe superar los 20 caracteres.");
        }
    }
}
