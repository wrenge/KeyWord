using KeyWord.Communication;
using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Server.Storage;

public interface IStorageContext
{
    DbSet<ClassicCredentialsStorageElement> ClassicCredentialsInfos { get; }
    DbSet<Device> Devices { get; }
    int SaveChanges();
}