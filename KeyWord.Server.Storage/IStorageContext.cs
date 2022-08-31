using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Server.Storage;

public interface IStorageContext
{
    DbSet<ClassicCredentialsInfo> ClassicCredentialsInfos { get; }
    DbSet<Device> Devices { get; }
    int SaveChanges();
}