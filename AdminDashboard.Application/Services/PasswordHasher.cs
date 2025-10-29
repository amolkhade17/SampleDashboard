using System.Security.Cryptography;
using System.Text;

namespace AdminDashboard.Application.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        // Create salt
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        // Create hash
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        // Combine salt and hash
        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Convert to base64
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            {
                Console.WriteLine($"[PasswordHasher] Password or hash is null/empty");
                return false;
            }
            
            Console.WriteLine($"[PasswordHasher] Input password: {password}");
            Console.WriteLine($"[PasswordHasher] Stored hash: {hash}");
            Console.WriteLine($"[PasswordHasher] Hash length: {hash.Length}");
            
            // Extract salt and hash
            var hashBytes = Convert.FromBase64String(hash);
            Console.WriteLine($"[PasswordHasher] Decoded hash bytes length: {hashBytes.Length} (expected: {SaltSize + HashSize})");
            
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            Console.WriteLine($"[PasswordHasher] Extracted salt (base64): {Convert.ToBase64String(salt)}");

            // Compute hash of provided password
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(HashSize);
            Console.WriteLine($"[PasswordHasher] Computed hash (base64): {Convert.ToBase64String(computedHash)}");
            
            // Extract stored hash part
            var storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);
            Console.WriteLine($"[PasswordHasher] Stored hash part (base64): {Convert.ToBase64String(storedHash)}");

            // Compare hashes
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != computedHash[i])
                {
                    Console.WriteLine($"[PasswordHasher] Hash mismatch at byte {i}");
                    return false;
                }
            }

            Console.WriteLine($"[PasswordHasher] Password verification SUCCESSFUL!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PasswordHasher] Exception: {ex.Message}");
            return false;
        }
    }
}
