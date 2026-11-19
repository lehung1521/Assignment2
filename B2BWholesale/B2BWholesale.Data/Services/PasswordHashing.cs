using System.Security.Cryptography;
using System.Text;

namespace B2BWholesale.Data.Services;

public static class PasswordHashing
{
    public static string ComputeSha256(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    public static bool VerifySha256(string? password, string? storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var expected = Encoding.ASCII.GetBytes(storedHash.Trim().ToUpperInvariant());
        var actual = Encoding.ASCII.GetBytes(ComputeSha256(password));
        return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
