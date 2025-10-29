using System;
using System.Security.Cryptography;

var password = "Admin@123";
const int SaltSize = 16;
const int HashSize = 32;
const int Iterations = 10000;

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
var passwordHash = Convert.ToBase64String(hashBytes);

Console.WriteLine("Password: Admin@123");
Console.WriteLine("Generated Hash:");
Console.WriteLine(passwordHash);
