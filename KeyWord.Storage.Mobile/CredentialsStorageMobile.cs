using System;
using System.Collections.Generic;
using System.Linq;
using KeyWord.Credentials;

namespace KeyWord.Storage.Mobile;

public class CredentialsStorageMobile : ICredentialsStorage
{
    public string? Password { get; set; }
    public string DbFilePath { get; }

    public CredentialsStorageMobile(string dbFilePath)
    {
        DbFilePath = dbFilePath;
    }

    public IReadOnlyList<CredentialsIdentity> GetIdentities()
    {
        using var dbContext = new CredentialsContext(DbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Select(x => x.ToCredentialsIdentity())
            .ToArray();
    }

    public ICredentialsInfo? GetInfo(int id)
    {
        if (Password == null)
            throw new ArgumentNullException(null, nameof(Password));
        if (id <= 0)
            throw new ArgumentException(null, nameof(id));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var info = dbContext.ClassicCredentialsInfos
            .FirstOrDefault(x => x.Id == id);
        
        // TODO: add decryption
        return info;
    }

    public bool SaveInfo(ICredentialsInfo info)
    {
        if (Password == null)
            throw new ArgumentNullException(null, nameof(Password));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var exists = dbContext.ClassicCredentialsInfos
            .Any(x => x.Identifier == info.Identifier && x.Login == info.Login);
        
        if (exists)
            return false;

        if (info is not ClassicCredentialsInfo classicInfo)
            return false;

        // TODO: add encryption
        dbContext.ClassicCredentialsInfos.Add(classicInfo);
        dbContext.SaveChanges();
        return true;
    }

    public bool UpdateInfo(int id, ICredentialsInfo info)
    {
        if (Password == null)
            throw new ArgumentNullException(null, nameof(Password));
        if (id <= 0)
            throw new ArgumentException(null, nameof(id));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var oldInfo = dbContext.ClassicCredentialsInfos.FirstOrDefault(x => x.Id == id);
        if (oldInfo == null)
            return false;
        if (info is not ClassicCredentialsInfo classicInfo)
            return false;

        var newInfo = new ClassicCredentialsInfo(classicInfo);
        newInfo.Id = id;
        // TODO: add encryption
        dbContext.Entry(oldInfo).CurrentValues.SetValues(newInfo);
        dbContext.SaveChanges();
        
        return true;
    }

    public bool DeleteInfo(int id)
    {
        if (Password == null)
            throw new ArgumentNullException(null, nameof(Password));
        if (id <= 0)
            throw new ArgumentException(null, nameof(id));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var info = dbContext.ClassicCredentialsInfos
            .FirstOrDefault(x => x.Id == id);
        
        if (info == null)
            return false;

        dbContext.ClassicCredentialsInfos.Remove(info);
        dbContext.SaveChanges();
        
        return true;
    }
    
    // TODO: add password validation
}