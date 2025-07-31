using ShorterURL.API.Data;
using System.Security.Cryptography;
using System.Text;

namespace ShorterURL.API.Services;

public class UrlShortenerService(ApplicationDbContext ctx)
{
    private const int DefaultCodeLength = 7;
    private const string Symbols = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int MaxAttempts = 15;

    private readonly ApplicationDbContext _ctx = ctx;

    public Task<string> GenerateUniqueCodeAsync(string? preferredCode)
    {
        // custom‑код
        if (!string.IsNullOrWhiteSpace(preferredCode))
        {
            preferredCode = preferredCode.ToLowerInvariant();
            if (preferredCode.Length is < 4 or > 20)
            {
                throw new ArgumentException("CustomCode must be between 4 and 20 characters.");
            }
            if (_ctx.Urls.Exists(u => u.Code == preferredCode))
            {
                throw new InvalidOperationException($"The code '{preferredCode}' is already in use.");
            }
            return Task.FromResult(preferredCode);
        }

        // случайный код
        var attempts = 0;
        while (attempts < MaxAttempts)
        {
            var generated = GenerateSecureRandomCode(DefaultCodeLength);
            if (!_ctx.Urls.Exists(x => x.Code == generated))
            {
                return Task.FromResult(generated);
            }
            attempts++;
        }
        throw new InvalidOperationException("Unable to generate a unique code after multiple attempts.");
    }

    private static string GenerateSecureRandomCode(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var sb = new StringBuilder(length);
        foreach (var b in bytes)
        {
            sb.Append(Symbols[b % Symbols.Length]);
        }
        return sb.ToString();
    }
}