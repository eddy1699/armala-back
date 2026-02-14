using System.Text.RegularExpressions;

namespace Armala.Auth.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email no puede estar vacío", nameof(value));

        value = value.Trim().ToLowerInvariant();

        if (value.Length > 255)
            throw new ArgumentException("Email no puede exceder 255 caracteres", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"Formato de email inválido: {value}", nameof(value));

        return new Email(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
