using KeyWord.Credentials;

namespace KeyWord.Server.Storage;

public class ServerStorage : IStorage
{
    private readonly string _dbFilePath;

    public ServerStorage(string dbFilePath)
    {
        _dbFilePath = dbFilePath;
    }

    public int Count
    {
        get
        {
            using var dbContext = new StorageContext(_dbFilePath);
            return dbContext.ClassicCredentialsInfos.Count();
        }
    }

    public IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Where(x => x.CreationTime > since)
            .ToArray();
    }

    public IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Where(x => x.CreationTime < since && x.ModificationTime != null && x.ModificationTime > since)
            .ToArray();
    }

    public IEnumerable<int> GetDeletedCredentials(DateTime since)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Where(x => x.RemoveTime != null && x.RemoveTime > since)
            .Select(x => x.Id)
            .ToArray();
    }

    public Device? FindDeviceById(string id)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.Devices.FirstOrDefault(x => x.Id == id);
    }

    public void AddDevice(Device device)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        dbContext.Devices.Add(device);
        dbContext.SaveChanges();
    }

    public IEnumerable<Device> GetDevices()
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.Devices.ToList();
    }

    public void AddCredentials(IEnumerable<ClassicCredentialsInfo> infos)
    {
        var infosQuery = infos.AsQueryable();
        var infosIds = infosQuery.Select(x => x.Id).ToArray();
        using var dbContext = new StorageContext(_dbFilePath);
        var existing = dbContext.ClassicCredentialsInfos
            .Where(x => infosIds.Contains(x.Id));
        
        var infosToAdd = infosQuery
            .Where(x => !existing.Any(y => y.Id == x.Id && y.CreationTime > x.CreationTime && y.ModificationTime > x.ModificationTime));
        
        dbContext.ClassicCredentialsInfos.AddRange(infosToAdd);
        dbContext.SaveChanges();
    }

    public void UpdateCredentials(IEnumerable<ClassicCredentialsInfo> infos)
    {
        var infosQuery = infos.AsQueryable();
        var infosIds = infosQuery.Select(y => y.Id).ToArray();
        using var dbContext = new StorageContext(_dbFilePath);
        var modified = dbContext.ClassicCredentialsInfos
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
        
        dbContext.SaveChanges();
    }

    public void DeleteCredentials(IEnumerable<int> infos)
    {
        var infosArray = infos.ToArray();
        var now = DateTime.Now;
        using var dbContext = new StorageContext(_dbFilePath);
        var removed = dbContext.ClassicCredentialsInfos
            .Where(x => infosArray.Contains(x.Id));
        foreach (var info in removed)
        {
            info.Name = info.Identifier = info.Login = info.Password = "";
            info.CreationTime = new DateTime();
            info.ModificationTime = new DateTime();
            info.RemoveTime = now;
        }
        
        dbContext.SaveChanges();
    }
}