using MediatR;
using log4net;

using Usuarios.Application.DTOs;

using Usuarios.Infrastructure.Interfaces;

namespace Usuarios.Infrastructure.Queries.QueryHandlers
{
    public class GetUserByNameQueryHandler : IRequestHandler<GetUserByNameQuery, UserByEmailDto>
    {
        private readonly IUserReadRepository _userReadRepository;

        public GetUserByNameQueryHandler(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        }

        public async Task<UserByEmailDto> Handle(GetUserByNameQuery request, CancellationToken cancellationToken)
        {

            try
            {
                var user = await _userReadRepository.GetByNameAsync(request.Name);

                if (user == null)
                {
                    return null;
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
