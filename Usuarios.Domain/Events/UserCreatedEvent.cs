using MediatR;

namespace Usuarios.Domain.Events
{
    public class UserCreatedEvent : INotification
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string RoleId { get; private set; }
        public string? Address { get; private set; } // Opcional
        public string? Phone { get; private set; } // Opcional

        public UserCreatedEvent(string id, string name, string lastName, string email, string roleId, string address, string phone)
        {
            Id = id;
            Name = name;
            LastName = lastName;
            Email = email;
            RoleId = roleId;
            Address = address;
            Phone = phone;
        }
    }
}
