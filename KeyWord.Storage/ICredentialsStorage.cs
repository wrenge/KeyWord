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
        void SaveInfo(ICredentialsInfo info);
        void UpdateInfo(int id, ICredentialsInfo info);
        void DeleteInfo(int id);
        bool IsPasswordCorrect();
        bool HasPassword();
        void ChangePassword(string newPassword);
    }
}