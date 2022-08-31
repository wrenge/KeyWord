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

    public IEnumerable<ClassicCredentialsInfo> GetAllCredentials()
    {
        return _storageContext.ClassicCredentialsInfos.ToArray();
    }

    public IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since)
    {
        return _storageContext.ClassicCredentialsInfos
            .Where(x => x.CreationTime > since)
            .ToArray();
    }

    public IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since)
    {
        return _storageContext.ClassicCredentialsInfos
            .Where(x => x.CreationTime <= since && x.ModificationTime != null && x.ModificationTime > since)
            .ToArray();
    }

    public IEnumerable<int> GetDeletedCredentials(DateTime since)
    {
        return _storageContext.ClassicCredentialsInfos
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

    public IEnumerable<Device> GetDevices()
    {
        return _storageContext.Devices.ToList();
    }

    public void AddCredentials(IEnumerable<ClassicCredentialsInfo> infos)
    {
        var infosQuery = infos.AsQueryable();
        var infosIds = infosQuery.Select(x => x.Id).ToArray();
        var existing = _storageContext.ClassicCredentialsInfos
            .Where(x => infosIds.Contains(x.Id));
        
        var infosToAdd = infosQuery
            .Where(x => !existing.Any(y => y.Id == x.Id && y.CreationTime > x.CreationTime && y.ModificationTime > x.ModificationTime));
        
        _storageContext.ClassicCredentialsInfos.AddRange(infosToAdd);
        _storageContext.SaveChanges();
    }

    public void UpdateCredentials(IEnumerable<ClassicCredentialsInfo> infos)
    {
        var infosQuery = infos.AsQueryable();
        var infosIds = infosQuery.Select(y => y.Id).ToArray();
        var modified = _storageContext.ClassicCredentialsInfos
            .Where(x => infosIds.Contains(x.Id));
        
        foreach (var info in modified)
        {
            var counterPart = infosQuery.First(x => x.Id == info.Id);
            info.Name = counterPart.Name;
            info.Identifier = counterPart.Identifier;
            info.Login = counterPart.Login;
            info.Password =  counterPart.Password;
            info.CreationTime = counterPart.CreationTime;
            info.ModificationTime = counterPart.ModificationTime;
        }
        
        _storageContext.SaveChanges();
    }

    public void DeleteCredentials(IEnumerable<int> infos)
    {
        var infosArray = infos.ToArray();
        var now = DateTime.Now;
        var removed = _storageContext.ClassicCredentialsInfos
            .Where(x => infosArray.Contains(x.Id));
        foreach (var info in removed)
        {
            info.Name = info.Identifier = info.Login = info.Password = "";
            info.CreationTime = new DateTime();
            info.ModificationTime = null;
            info.RemoveTime = now;
        }
        
        _storageContext.SaveChanges();
    }
}