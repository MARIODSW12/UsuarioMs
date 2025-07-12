using FluentValidation;

using Usuarios.Application.DTOs;

namespace Usuarios.Application.Validations
{


    public class UpdateUserRoleDtoValidation : AbstractValidator<UpdateUserRoleDto>
    {
        public UpdateUserRoleDtoValidation()
        {
            RuleFor(x => x.NewRoleId)
                .NotEmpty().WithMessage("El nuevo RoleId es obligatorio.")
                .Matches("^[a-fA-F0-9]{24}$").WithMessage("El RoleId debe ser un ObjectId válido.");
        }
    }
}
