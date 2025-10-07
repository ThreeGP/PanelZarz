using System;
using System.Security.Cryptography;
using System.Text;

namespace IndustrialPanel.Services;

public class PasswordHashService
{
    // Global seed (pepper)
    private readonly byte[] _seedKey;

    public PasswordHashService(string? seed = null)
    {
        // Resolve seed source
        var resolved = seed
            ?? Environment.GetEnvironmentVariable("INDUSTRIAL_PANEL_SEED")
            ?? "DefaultPepper_v1"; // demo fallback
        _seedKey = Encoding.UTF8.GetBytes(resolved);
    }

    public string GenerateSalt()
    {
        // Random 32B salt
        byte[] saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }
    
    public string HashPassword(string password, string salt)
    {
        // HMAC-SHA256 with seed
        using var hmac = new HMACSHA256(_seedKey);
        var messageBytes = Encoding.UTF8.GetBytes($"{password}:{salt}");
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }
    
    public bool VerifyPassword(string password, string salt, string storedHash)
    {
        string hashToVerify = HashPassword(password, salt);
        try
        {
            var a = Convert.FromBase64String(hashToVerify);
            var b = Convert.FromBase64String(storedHash);
            // Constant-time compare
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
        catch
        {
            // Non-base64 fallback
            return hashToVerify == storedHash;
        }
    }
}
