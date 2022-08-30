using System.Collections.Generic;
using KeyWord.Credentials;

namespace KeyWord.Storage
{
    public interface ICredentialsStorage
    {
        string Password { set; }
        int Count { get; }
        IReadOnlyList<CredentialsIdentity> GetIdentities();
        ICredentialsInfo? FindInfo(int id);
        bool SaveInfo(ICredentialsInfo info);
        bool UpdateInfo(int id, ICredentialsInfo info);
        bool DeleteInfo(int id);
        bool IsPasswordCorrect();
        bool HasPassword();
        void ChangePassword(string newPassword);
    }
}