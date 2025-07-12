using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Application.Commands
{
    public class UpdateUserRoleCommand : IRequest<bool>
    {
        public string UserId { get; }
        public UpdateUserRoleDto UserRoleDto { get; }

        public UpdateUserRoleCommand(string userId, UpdateUserRoleDto userRoleDto)
        {
            UserId = userId;
            UserRoleDto = userRoleDto;
        }
    }
}
