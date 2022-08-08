using KeyWord.Credentials;

namespace KeyWord.Server.Storage;

public interface IStorage
{
    int Count { get; }
    IEnumerable<ClassicCredentialsInfo> GetAllCredentials();
    IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since);
    IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since);
    IEnumerable<int> GetDeletedCredentials(DateTime since);
    Device? FindDeviceById(string id);
    void AddDevice(Device device);
    IEnumerable<Device> GetDevices();

    void AddCredentials(IEnumerable<ClassicCredentialsInfo> infos);
    void UpdateCredentials(IEnumerable<ClassicCredentialsInfo> infos);
    void DeleteCredentials(IEnumerable<int> infos);
}