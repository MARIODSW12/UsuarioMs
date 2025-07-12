using MediatR;

namespace Usuarios.Domain.Events
{
    public class UserRoleUpdatedEvent : INotification
    {
        public string UserId { get; }
        public string NewRoleId { get; }

        public UserRoleUpdatedEvent(string userId, string newRoleId)
        {
            UserId = userId;
            NewRoleId = newRoleId;
        }
    }
}
