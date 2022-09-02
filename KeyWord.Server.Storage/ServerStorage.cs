using KeyWord.Communication;
using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Server.Storage;

public class ServerStorage : IStorage
{
    private readonly StorageContext _storageContext;

    public ServerStorage(StorageContext storageContext)
    {
        _storageContext = storageContext;
    }

    public int Count => _storageContext.ClassicCredentialsInfos.Count();

    public IEnumerable<ClassicCredentialsInfo> GetAllCredentials(string authId)
    {
        return _storageContext.ClassicCredentialsInfos.ToArray();
    }

    public IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since, string authId)
    {
        return _storageContext.ClassicCredentialsInfos
            .Where(x => x.AuthId == authId)
            .Where(x => x.CreationTime > since)
            .ToArray();
    }

    public IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since, string authId)
    {
        return _storageContext.ClassicCredentialsInfos
            .Where(x => x.AuthId == authId)
            .Where(x => x.CreationTime <= since && x.ModificationTime != null && x.ModificationTime > since)
            .ToArray();
    }

    public IEnumerable<int> GetDeletedCredentials(DateTime since, string authId)
    {
        return _storageContext.ClassicCredentialsInfos
            .Where(x => x.AuthId == authId)
            .Where(x => x.RemoveTime != null && x.RemoveTime > since)
            .Select(x => x.Id)
            .ToArray();
    }

    public Device? FindDeviceById(string id)
    {
        return _storageContext.Devices.FirstOrDefault(x => x.Id == id);
    }

    public void AddDevice(Device device)
    {
        _storageContext.Devices.Add(device);
        _storageContext.SaveChanges();
    }

    public bool DeleteDevice(string id)
    {
        var device = _storageContext.Devices.FirstOrDefault(x => x.Id == id);
        if (device == null)
            return false;
        
        _storageContext.Devices.Remove(device);
        _storageContext.SaveChanges();
        return true;
    }

    public bool RenameDevice(string id, string name)
    {
        var device = _storageContext.Devices.FirstOrDefault(x => x.Id == id);
        if (device == null)
            return false;

        device.Name = name;
        _storageContext.SaveChanges();
        return true;
    }

    public IEnumerable<Device> GetDevices()
    {
        return _storageContext.Devices.ToList();
    }

    public void AddCredentials(IEnumerable<ClassicCredentialsInfo> infos, string authId)
    {
        var infosQuery = infos.AsQueryable();
        var infosIds = infosQuery.Select(x => x.Id).ToArray();
        var existing = _storageContext.ClassicCredentialsInfos
            .Where(x => infosIds.Contains(x.Id));
        
        var infosToAdd = infosQuery
            .Where(x => !existing.Any(y => y.Id == x.Id && y.CreationTime > x.CreationTime && y.ModificationTime > x.ModificationTime));
        
        _storageContext.ClassicCredentialsInfos.AddRange(
            infosToAdd.Select(x => new ClassicCredentialsStorageElement(x, authId)));
        _storageContext.SaveChanges();
    }

    public void UpdateCredentials(IEnumerable<ClassicCredentialsInfo> infos, string authId)
    {
        var infosQuery = infos.AsQueryable();
        var infosIds = infosQuery.Select(y => y.Id).ToArray();
        var modified = _storageContext.ClassicCredentialsInfos
            .Where(x => x.AuthId == authId)
            .Where(x => infosIds.Contains(x.Id));
        
        foreach (var info in modified)
        {
            var counterPart = infosQuery.First(x => x.Id == info.Id);
            _storageContext.Entry(info).CurrentValues.SetValues(new ClassicCredentialsStorageElement(counterPart, authId));
        }
        
        _storageContext.SaveChanges();
    }

    public void DeleteCredentials(IEnumerable<int> infos, string authId)
    {
        var infosArray = infos.ToArray();
        var now = DateTime.Now;
        var removed = _storageContext.ClassicCredentialsInfos
            .Where(x => x.AuthId == authId)
            .Where(x => infosArray.Contains(x.Id));
        foreach (var info in removed)
        {
            _storageContext.Entry(info).CurrentValues
                .SetValues(new ClassicCredentialsStorageElement(authId) {Id = info.Id, RemoveTime = now});
        }
        
        _storageContext.SaveChanges();
    }
}