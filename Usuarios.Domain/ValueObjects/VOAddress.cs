
namespace Usuarios.Domain.ValueObjects
{
    public class VOAddress
    {
        public string Value { get; private set; }

        public VOAddress(string value)
        {
            //if (string.IsNullOrWhiteSpace(value))
            //    throw new ArgumentException("La dirección no puede estar vacía.");

            Value = value;
        }

        public override string ToString() => Value;
    }
}
