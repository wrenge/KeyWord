using Xunit;

namespace KeyWord.IntegrationTests;

public class RegisterControllerIntegrationTests : IClassFixture<TestingWebAppFactory<Program>>
{
    private readonly HttpClient _client;
    public RegisterControllerIntegrationTests(TestingWebAppFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task TestRegister()
    {
        var admin = new Admin.Networking.RegisterService(_client);
        var clientDiscovery = new Client.Network.DiscoveryService();

        await admin.StartNewRegistration();
        var token = await admin.RequestNewToken();
        Assert.NotNull(token.Token);
        var deviceRequestTask = admin.RequestDeviceCandidate();

        var hostResponse = await clientDiscovery.DiscoverServer(token.ServerPort, token.Token, TimeSpan.FromSeconds(60));
        Assert.NotNull(hostResponse);
        var host = hostResponse!;
        var client = new Client.Network.RegisterService(_client);

        var deviceId = "MockDeviceId";
        var deviceName = "MockDevice";
        var registerTask = client.TryRegister(deviceId, deviceName, host, token.Token);

        var device = await deviceRequestTask;
        
        Assert.Equal(deviceId, device.Id);
        Assert.Equal(deviceName, device.Name);
        Assert.Equal(token.Token, device.Token);

        await Task.Delay(TimeSpan.FromSeconds(1));
        await admin.ApprovePendingDevice();

        Assert.Equal(await Task.WhenAny(registerTask, Task.Delay(TimeSpan.FromSeconds(1))), registerTask);
        Assert.True(registerTask.Result);
    }
}