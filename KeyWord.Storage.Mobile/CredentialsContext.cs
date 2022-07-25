using System.IO;
using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Storage.Mobile;

internal sealed class CredentialsContext : DbContext
{
    public DbSet<ClassicCredentialsInfo> ClassicCredentialsInfos { get; set; }
    public string DbFilePath { get; }

    public CredentialsContext (string dbFilePath)
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
    }
}