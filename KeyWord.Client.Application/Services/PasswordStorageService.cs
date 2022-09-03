using System.Security.Cryptography;
using System.Text;

namespace KeyWord.Client.Application.Services;

public class PasswordStorageService
{
    // TODO move to constants
    private const string PasswordStorageKey = "password";
    private const int PasswordLength = 16;
    private Encoding PasswordEncoding => Encoding.Default;

    public string Get()
    {
        var task = SecureStorage.GetAsync(PasswordStorageKey);
        task.Wait();
        return task.Result;
    }

    public async Task<string> GetAsync()
    {
        return await SecureStorage.GetAsync(PasswordStorageKey);
    }

    public void Set(string value)
    {
        var task = SetAsync(value);
        task.Wait();
    }
    
    public async Task SetAsync(string value)
    {
        await SecureStorage.SetAsync(PasswordStorageKey, value);
    }

    public string Generate()
    {
        var encoding = PasswordEncoding;
        var bytesPerChar = encoding.GetMaxByteCount(1);
        var passwordBytes = RandomNumberGenerator.GetBytes(PasswordLength * bytesPerChar);
        var password = encoding.GetString(passwordBytes);
        return password;
    }
}