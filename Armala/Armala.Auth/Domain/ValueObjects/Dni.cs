using System.Text.RegularExpressions;

namespace Armala.Auth.Domain.ValueObjects;

public sealed record Dni
{
    private static readonly Regex DniRegex = new(@"^\d{8}$", RegexOptions.Compiled);

    public string Value { get; }

    private Dni(string value)
    {
        Value = value;
    }

    public static Dni Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DNI no puede estar vacío", nameof(value));

        value = value.Trim();

        if (!DniRegex.IsMatch(value))
            throw new ArgumentException("DNI debe tener exactamente 8 dígitos", nameof(value));

        return new Dni(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(Dni dni) => dni.Value;
}
