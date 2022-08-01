using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Server.Storage;

public class StorageContext : DbContext
{
    public DbSet<ClassicCredentialsInfo> ClassicCredentialsInfos { get; set; }
    public DbSet<Device> Devices { get; set; }
    public string DbFilePath { get; }

    public StorageContext (string dbFilePath)
    {
        DbFilePath = dbFilePath;
        SQLitePCL.Batteries_V2.Init();
        this.Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Filename={DbFilePath}");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassicCredentialsInfo>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Device>()
            .HasKey(x => x.Id);
    }
}