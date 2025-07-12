
namespace Usuarios.Application.DTOs
{
    public class CreateUserDto
    {
        public required string Name { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string RoleId { get; set; }
        public string? Address { get; init; }
        public string? Phone { get; init; }
    }
}
