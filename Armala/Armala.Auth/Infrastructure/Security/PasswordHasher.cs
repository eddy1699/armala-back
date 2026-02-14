using BCrypt.Net;

namespace Armala.Auth.Infrastructure.Security;

/// <summary>
/// Implementación de hash de contraseñas usando BCrypt
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // Factor de trabajo (más alto = más seguro pero más lento)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña no puede estar vacío", nameof(passwordHash));

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehash(string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(passwordHash, WorkFactor);
        }
        catch
        {
            return true;
        }
    }
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    bool NeedsRehash(string passwordHash);
}
