using MediatR;

using Usuarios.Application.DTOs;

namespace Usuarios.Infrastructure.Queries
{
    public class GetUserByIdQuery : IRequest<UserByEmailDto>
    {
        public string Id { get; set; }

        public GetUserByIdQuery(string id)
        {
            Id = id;
        }
    }
}
