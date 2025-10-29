using System;
using System.Security.Cryptography;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run <password>");
    return;
}

string password = args[0];
string hash = HashPassword(password);
Console.WriteLine(hash);

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
