
namespace Usuarios.Domain.ValueObjects
{
    public class VOId
    {
        public string Value { get; private set; }

        public VOId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El ID de usuario no puede estar vacío.");

            if (!Guid.TryParse(value, out _))
                throw new ArgumentException("El ID de usuario debe ser un GUID válido.");

            Value = value;
        }

        //public static VOId Generate() => new VOId(Guid.NewGuid().ToString());

        public override string ToString() => Value;
    }
}
