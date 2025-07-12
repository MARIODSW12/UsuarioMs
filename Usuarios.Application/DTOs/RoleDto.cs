
namespace Usuarios.Application.DTOs
{
    public class RoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Permissions { get; set; }
    }
}
