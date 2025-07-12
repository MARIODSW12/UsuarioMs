using MediatR;

using Usuarios.Domain.ValueObjects;

namespace Usuarios.Domain.Events
{
    public class UserUpdatedEvent : INotification
    {
        public VOId UserId { get; }
        public VOName Name { get; }
        public VOLastName LastName { get; }
        public VOAddress Address { get; }
        public VOPhone Phone { get; }

        public UserUpdatedEvent(VOId userId, VOName name, VOLastName lastName, VOAddress address, VOPhone phone)
        {
            UserId = userId;
            Name = name;
            LastName = lastName;
            Address = address;
            Phone = phone;
        }
    }
}
