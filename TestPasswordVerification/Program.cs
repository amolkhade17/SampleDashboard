using System;
using System.Security.Cryptography;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string password = "Admin@123";
        
        // Get hash from database
        string connectionString = "Server=localhost;Database=AdminDashboardDB;Trusted_Connection=True;TrustServerCertificate=True;";
        string storedHash = "";
        
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT PasswordHash FROM Users WHERE Username = 'admin'";
                storedHash = command.ExecuteScalar()?.ToString() ?? "";
            }
        }
        
        Console.WriteLine($"Stored Hash: {storedHash}");
        Console.WriteLine($"Hash Length: {storedHash.Length}");
        Console.WriteLine();
        
        // Verify password
        bool isValid = VerifyPassword(password, storedHash);
        Console.WriteLine($"Password '{password}' verification: {(isValid ? "SUCCESS" : "FAILED")}");
        
        if (!isValid)
        {
            // Try hashing the same password and see if it matches format
            string newHash = HashPassword(password);
            Console.WriteLine($"\nNew Hash: {newHash}");
            Console.WriteLine($"New Hash Length: {newHash.Length}");
            
            bool testVerify = VerifyPassword(password, newHash);
            Console.WriteLine($"Test Verification: {(testVerify ? "SUCCESS" : "FAILED")}");
        }
    }
    
    static string HashPassword(string password)
    {
        const int SaltSize = 16;
        const int HashSize = 32;
        const int Iterations = 10000;

        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }
    
    static bool VerifyPassword(string password, string hash)
    {
        try
        {
            const int SaltSize = 16;
            const int HashSize = 32;
            const int Iterations = 10000;

            var hashBytes = Convert.FromBase64String(hash);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != computedHash[i])
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying password: {ex.Message}");
            return false;
        }
    }
}
