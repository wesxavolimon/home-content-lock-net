using System.Security.Cryptography;
using System.Text;
using HomeContentLock.Domain.Exceptions;
using HomeContentLock.Domain.Interfaces;

namespace HomeContentLock.Infrastructure.Services;

public class PasswordValidator : IPasswordValidator
{
    private readonly Dictionary<string, string> _passwordStore = new();

    public async Task<bool> ValidateAsync(string password)
    {
        return await Task.FromResult(_passwordStore.Values.Any(hash => VerifyHash(password, hash)));
    }

    public async Task SavePasswordHashAsync(string passwordHash)
    {
        _passwordStore["admin"] = passwordHash;
        await Task.CompletedTask;
    }

    private bool VerifyHash(string password, string hash)
    {
        using var sha256 = SHA256.Create();
        var hashedInput = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return hashedInput == hash;
    }
}
