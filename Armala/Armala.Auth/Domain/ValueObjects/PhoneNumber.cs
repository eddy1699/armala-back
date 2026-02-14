using PhoneNumbers;

namespace Armala.Auth.Domain.ValueObjects;

public sealed record PhoneNumber
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    public string Value { get; }
    public string CountryCode { get; }
    public string NationalNumber { get; }

    private PhoneNumber(string value, string countryCode, string nationalNumber)
    {
        Value = value;
        CountryCode = countryCode;
        NationalNumber = nationalNumber;
    }

    public static PhoneNumber Create(string value, string defaultRegion = "PE")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Número de teléfono no puede estar vacío", nameof(value));

        try
        {
            var phoneNumber = PhoneUtil.Parse(value, defaultRegion);

            if (!PhoneUtil.IsValidNumber(phoneNumber))
                throw new ArgumentException($"Número de teléfono inválido: {value}", nameof(value));

            var formattedNumber = PhoneUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            var countryCode = $"+{phoneNumber.CountryCode}";
            var nationalNumber = phoneNumber.NationalNumber.ToString();

            return new PhoneNumber(formattedNumber, countryCode, nationalNumber);
        }
        catch (NumberParseException ex)
        {
            throw new ArgumentException($"Error al parsear número de teléfono: {value}", nameof(value), ex);
        }
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
