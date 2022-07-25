using System;

namespace KeyWord.Credentials
{
    public interface ICredentialsInfo
    {
        int Id { get; set; }
        string Login { get; set; }
        string Password { get; }
        DateTime CreationTime { get; set; }
        DateTime ModificationTime { get; set; }
    }
}