using KeyWord.Server.Storage;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KeyWord.IntegrationTests;

public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<StorageContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<StorageContext>(options => options.UseSqlite("DataSource=file::memory:"));
            
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