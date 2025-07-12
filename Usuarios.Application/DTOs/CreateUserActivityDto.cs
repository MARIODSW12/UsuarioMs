
namespace Usuarios.Application.DTOs
{
    public class CreateUserActivityDto
    {
        public required string UserId { get; set; } = default!;
        public required string Action { get; set; } = default!;
    }
}
