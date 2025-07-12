using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Application.Commands
{

    public class CreateUserCommand : IRequest<String>
    {
        public CreateUserDto UserDto { get; }

        public CreateUserCommand(CreateUserDto userDto)
        {
            UserDto = userDto ?? throw new ArgumentNullException(nameof(userDto));
        }
    }
}
