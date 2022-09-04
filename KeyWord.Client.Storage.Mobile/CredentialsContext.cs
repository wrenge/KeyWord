using KeyWord.Credentials;
using Microsoft.EntityFrameworkCore;

namespace KeyWord.Client.Storage.Mobile
{
    internal sealed class CredentialsContext : DbContext
    {
        public DbSet<ClassicCredentialsInfo> ClassicCredentialsInfos { get; set; } = null;
        public DbSet<KeyValueEntry> KeyValues { get; set; } = null;
        public string DbFileName { get; }
        private IDatabasePath _databasePath;

        public CredentialsContext (IDatabasePath databasePath, string dbFileName)
        {
            DbFileName = dbFileName;
            _databasePath = databasePath;
            SQLitePCL.Batteries_V2.Init();
            this.Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={_databasePath.GetPath(DbFileName)}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClassicCredentialsInfo>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<KeyValueEntry>()
                .HasKey(x => x.Key);
        }
    }
}