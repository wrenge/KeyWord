using KeyWord.Server.Storage;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace KeyWord.IntegrationTests;

public class TestingWebAppFactory : WebApplicationFactory<Program>
{
    public static readonly InMemoryDatabaseRoot DbRoot = new InMemoryDatabaseRoot();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<StorageContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<StorageContext>(options => options.UseInMemoryDatabase("server_storage", DbRoot));

            // var sp = services.BuildServiceProvider();
            // using var scope = sp.CreateScope();
            // using var appContext = scope.ServiceProvider.GetRequiredService<StorageContext>();
            // try
            // {
            //     appContext.Database.Migrate();
            // }
            // catch (Exception ex)
            // {
            //     //Log errors or do anything you think it's needed
            //     throw;
            // }
        });
    }
}