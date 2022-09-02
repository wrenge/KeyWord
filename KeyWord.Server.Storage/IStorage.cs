using KeyWord.Communication;
using KeyWord.Credentials;

namespace KeyWord.Server.Storage;

public interface IStorage
{
    int Count { get; }
    IEnumerable<ClassicCredentialsInfo> GetAllCredentials(string authId);
    IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since, string authId);
    IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since, string authId);
    IEnumerable<int> GetDeletedCredentials(DateTime since, string authId);
    Device? FindDeviceById(string id);
    void AddDevice(Device device);
    bool DeleteDevice(string id);
    bool RenameDevice(string id, string name);
    IEnumerable<Device> GetDevices();

    void AddCredentials(IEnumerable<ClassicCredentialsInfo> infos, string authId);
    void UpdateCredentials(IEnumerable<ClassicCredentialsInfo> infos, string authId);
    void DeleteCredentials(IEnumerable<int> infos, string authId);
}