using MediatR;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserByEmailDto>
    {
        private readonly IUserReadRepository _userReadRepository;

        public GetUserByIdQueryHandler(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        }

        public async Task<UserByEmailDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {

            try
            {
                var user = await _userReadRepository.GetByIdAsync(request.Id);

                if (user == null)
                {
                    throw new KeyNotFoundException("Usuario no encontrado.");
                }

                var userDto = new UserByEmailDto
                {
                    userId = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email,
                    RoleId = user.roleId,
                    Address = user.Address,
                    Phone = user.Phone
                };

                return userDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
