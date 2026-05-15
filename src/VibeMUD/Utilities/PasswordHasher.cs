namespace VibeMUD.Utilities;

using BCrypt.Net;

public class PasswordHasher
{
    private const int BcryptWorkFactor = 12;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty");

        return BCrypt.HashPassword(password, BcryptWorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            return BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
