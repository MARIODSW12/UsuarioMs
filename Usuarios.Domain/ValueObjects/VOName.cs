
namespace Usuarios.Domain.ValueObjects
{
    public class VOName
    {
        public string Value { get; private set; }

        public VOName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre no puede estar vacío.");

            Value = value;
        }

        public override string ToString() => Value;
    }
}
