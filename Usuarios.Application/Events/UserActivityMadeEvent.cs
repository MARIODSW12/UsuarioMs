
namespace Usuarios.Application.Events
{
    public class UserActivityMadeEvent
    {
        public string UserId { get; set; }
        public string Action { get; set; } //Ejemplo: "USER_UPDATED", "LOGIN", "PASSWORD_CHANGED"
        public DateTime Timestamp { get; set; }

        public UserActivityMadeEvent(string userId, string action, DateTime timestamp)
        {
            UserId = userId;
            Action = action;
            Timestamp = timestamp;
        }
    }
}
