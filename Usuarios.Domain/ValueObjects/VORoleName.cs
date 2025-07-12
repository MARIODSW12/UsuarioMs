
namespace Usuarios.Domain.ValueObjects
{
    public class VORoleName
    {
        public string Value { get; private set; }

        public VORoleName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre del rol no puede estar vacío.");

            Value = value;
        }

        public override string ToString() => Value;
    }
}
