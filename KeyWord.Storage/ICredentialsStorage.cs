using System.Collections.Generic;
using KeyWord.Credentials;

namespace KeyWord.Storage
{
    public interface ICredentialsStorage
    {
        string Password { set; }
        IReadOnlyList<CredentialsIdentity> GetIdentities();
        ICredentialsInfo? GetInfo(int id);
        bool SaveInfo(ICredentialsInfo info);
        bool UpdateInfo(int id, ICredentialsInfo info);
        bool DeleteInfo(int id);
    }
}