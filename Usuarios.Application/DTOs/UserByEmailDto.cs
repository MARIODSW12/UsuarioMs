
namespace Usuarios.Application.DTOs
{
    public class UserByEmailDto
    {
        public string userId { get; init; }
        public string Name { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public string RoleId { get; set; }
        public string Address { get; init; }
        public string Phone { get; init; }
    }
}
