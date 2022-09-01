using KeyWord.Server.Controllers;

namespace KeyWord.Server.Services;

public class RegisterService : IDisposable
{
    public RegisterSession? CurrentSession { get; set; }

    public void Dispose()
    {
        CurrentSession?.Dispose();
    }
}