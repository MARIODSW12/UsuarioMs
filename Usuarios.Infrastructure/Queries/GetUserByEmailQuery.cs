using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Infrastructure.Queries
{
    public class GetUserByEmailQuery : IRequest<UserByEmailDto>
    {
        public string Email { get; set; }

        public GetUserByEmailQuery(string email)
        {
            Email = email;
        }
    }
}
