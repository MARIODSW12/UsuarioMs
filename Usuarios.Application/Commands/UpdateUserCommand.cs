using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Application.Commands
{
    public class UpdateUserCommand : IRequest<bool>
    {
        public UpdateUserDto UserDto { get; }
        public string id { get; }

        public UpdateUserCommand(UpdateUserDto userDto, string id)
        {
            UserDto = userDto;
            this.id = id;
        }
    }
}
