using Usuarios.Domain.ValueObjects;

namespace Usuarios.Domain.Aggregates
{
    public class User
    {
        public VOId Id { get; private set; }
        public VOName Name { get; private set; }
        public VOLastName LastName { get; private set; }
        public VOEmail Email { get; private set; }
        public VORoleId RoleId { get; private set; }
        public VOAddress? Address { get; private set; } // Opcional
        public VOPhone? Phone { get; private set; } // Opcional

        private User() { }

        public User(VOId id, VOName name, VOLastName lastName, VOEmail email, VORoleId roleid, VOAddress? address = null, VOPhone? phone = null)
        {
            Id = id;
            Name = name;
            LastName = lastName;
            Email = email;
            Address = address;
            Phone = phone;
            RoleId = roleid;
        }

        public void Update(string? name, string? lastName, string? address = null, string? phone = null)
        {
            if (name != null)
            {
                this.Name = new VOName(name);
            }

            if (lastName != null)
            {
                this.LastName = new VOLastName(lastName);
            }

            if (address != null)
            {
                this.Address = new VOAddress(address);
            }

            if (phone != null)
            {
                this.Phone = new VOPhone(phone);
            }
        }

        public void UpdateRole(string newRoleId)
        {
            RoleId = new VORoleId(newRoleId);
        }
    }
}