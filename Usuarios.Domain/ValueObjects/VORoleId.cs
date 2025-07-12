namespace Usuarios.Domain.ValueObjects
{
    public class VORoleId
    {
        public string Value { get; private set; }

        public VORoleId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El ID del rol no puede estar vacío.");

            Value = value;
        }

        public override string ToString() => Value;
    }
}