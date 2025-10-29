using System;
using System.Security.Cryptography;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        // Hash the password
        string password = "Admin@123";
        string passwordHash = HashPassword(password);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {passwordHash}");
        Console.WriteLine();
        
        // Update database
        string connectionString = "Server=localhost;Database=AdminDashboardDB;Trusted_Connection=True;TrustServerCertificate=True;";
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username = 'admin'";
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Updated {rowsAffected} user(s) in database.");
                Console.WriteLine("Admin password has been reset to: Admin@123");
            }
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
}
