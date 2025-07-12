using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Infrastructure.Queries
{
    public class GetUserByNameQuery : IRequest<UserByEmailDto>
    {
        public string Name { get; set; }

        public GetUserByNameQuery(string name)
        {
            Name = name;
        }
    }
}
