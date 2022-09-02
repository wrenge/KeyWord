using KeyWord.Communication;
using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Server.Storage;

public class StorageContext : DbContext, IStorageContext
{
    public DbSet<ClassicCredentialsStorageElement> ClassicCredentialsInfos { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;

    public StorageContext(DbContextOptions<StorageContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassicCredentialsStorageElement>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ClassicCredentialsStorageElement>()
            .HasIndex(e => e.AuthId);

        modelBuilder.Entity<Device>()
            .HasKey(x => x.Id);
    }
}