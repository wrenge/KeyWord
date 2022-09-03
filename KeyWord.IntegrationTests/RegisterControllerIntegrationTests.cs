using KeyWord.Server.Storage;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KeyWord.IntegrationTests;

public class RegisterControllerIntegrationTests : IClassFixture<TestingWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly StorageContext _context;

    public RegisterControllerIntegrationTests(TestingWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _context = new StorageContext(new DbContextOptionsBuilder<StorageContext>()
            .UseInMemoryDatabase(TestingWebAppFactory.DbName, TestingWebAppFactory.DbRoot).Options);
        _context.Database.EnsureCreated();
    }
    
    [Fact]
    public async Task TestRegister()
    {
        var admin = new Admin.Services.RegisterService(_client);
        var clientDiscovery = new Client.Services.DiscoveryService();

        await admin.StartNewRegistration();
        var token = await admin.RequestNewToken();
        Assert.NotNull(token.Token);
        var deviceRequestTask = admin.RequestDeviceCandidate();

        var hostResponse = await clientDiscovery.DiscoverServer(token.ServerPort, token.Token, TimeSpan.FromSeconds(60));
        Assert.NotNull(hostResponse);
        var host = hostResponse!;
        var client = new Client.Services.RegisterService(_client);

        var deviceId = "MockDeviceId";
        var deviceName = "MockDevice";
        var registerTask = client.TryRegister(deviceId, deviceName, token.Token);

        var device = await deviceRequestTask;
        
        Assert.Equal(deviceId, device.Id);
        Assert.Equal(deviceName, device.Name);
        Assert.Equal(token.Token, device.Token);

        await Task.Delay(TimeSpan.FromSeconds(1));
        await admin.ApprovePendingDevice();

        Assert.Equal(await Task.WhenAny(registerTask, Task.Delay(TimeSpan.FromSeconds(10))), registerTask);
        Assert.True(registerTask.Result);
    }

    public void Dispose()
    {
        _client.Dispose();
        _context.Database.EnsureDeleted();
    }
}