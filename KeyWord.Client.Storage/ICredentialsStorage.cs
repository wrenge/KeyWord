using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KeyWord.Credentials;

namespace KeyWord.Client.Storage
{
    public interface ICredentialsStorage
    {
        string Password { set; }
        int Count { get; }
        IReadOnlyList<CredentialsIdentity> GetIdentities();
        ICredentialsInfo FindInfo(int id);
        void SaveInfo(ICredentialsInfo info);
        void UpdateInfo(int id, ICredentialsInfo info);
        void DeleteInfo(int id);
        bool IsPasswordCorrect();
        bool HasPassword();
        void ChangePassword(string newPassword);
        void SetKey(string key, string value);
        string GetKeyOr(string key, string defaultValue = null);
        
        IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since);
        IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since);
        IEnumerable<int> GetDeletedCredentials(DateTime since);
        
        void SetAddedCredentials(IEnumerable<ClassicCredentialsInfo> list);
        void SetModifiedCredentials(IEnumerable<ClassicCredentialsInfo> list);
        void SetDeletedCredentials(IEnumerable<int> list);
    }
}