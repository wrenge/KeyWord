using KeyWord.Credentials;

namespace KeyWord.Server.Storage;

public class ServerStorage : IStorage
{
    private readonly string _dbFilePath;

    public ServerStorage(string dbFilePath)
    {
        _dbFilePath = dbFilePath;
    }

    public IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Where(x => x.CreationTime > since);
    }

    public IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Where(x => x.CreationTime < since && x.ModificationTime != null && x.ModificationTime > since);
    }

    public IEnumerable<int> GetDeletedCredentials(DateTime since)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Where(x => x.RemoveTime != null && x.RemoveTime > since)
            .Select(x => x.Id);
    }

    public Device? FindDeviceById(string id)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        return dbContext.Devices.FirstOrDefault(x => x.Id == id);
    }

    public void AddCredentials(IEnumerable<ClassicCredentialsInfo> infos)
    {
        using var dbContext = new StorageContext(_dbFilePath);
        var existing = dbContext.ClassicCredentialsInfos
            .Where(x => infos.Any(y => x.Id == y.Id));
        
        var infosToAdd = infos
            .Where(x => existing.Any(y => x.CreationTime > y.CreationTime && x.ModificationTime > y.ModificationTime));
        
        dbContext.ClassicCredentialsInfos.AddRange(infosToAdd);
        dbContext.SaveChanges();
    }

    public void UpdateCredentials(IEnumerable<ClassicCredentialsInfo> infos)
    {
        var infosList = infos.ToList();
        var now = DateTime.Now;
        using var dbContext = new StorageContext(_dbFilePath);
        var modified = dbContext.ClassicCredentialsInfos
            .IntersectBy(infosList.ToList().Select(x => x.Id), x => x.Id);
        
        foreach (var info in modified)
        {
            var counterPart = infosList.First(x => x.Id == info.Id);
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
        var now = DateTime.Now;
        using var dbContext = new StorageContext(_dbFilePath);
        var removed = dbContext.ClassicCredentialsInfos.IntersectBy(infos, x => x.Id);
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