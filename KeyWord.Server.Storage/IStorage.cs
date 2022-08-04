using KeyWord.Credentials;

namespace KeyWord.Server.Storage;

public interface IStorage
{
    IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since);
    IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since);
    IEnumerable<int> GetDeletedCredentials(DateTime since);
    Device? FindDeviceById(string id);

    void AddCredentials(IEnumerable<ClassicCredentialsInfo> infos);
    void UpdateCredentials(IEnumerable<ClassicCredentialsInfo> infos);
    void DeleteCredentials(IEnumerable<int> infos);
}