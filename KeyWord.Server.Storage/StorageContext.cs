﻿using KeyWord.Communication;
using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Server.Storage;

public class StorageContext : DbContext, IStorageContext
{
    public DbSet<ClassicCredentialsInfo> ClassicCredentialsInfos { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;

    public StorageContext(DbContextOptions<StorageContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassicCredentialsInfo>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Device>()
            .HasKey(x => x.Id);
    }
}