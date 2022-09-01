using System.Web;
using KeyWord.Admin.Networking;
using KeyWord.Communication;
using KeyWord.Server.Storage;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KeyWord.IntegrationTests;

public class DeviceControllerTest : IClassFixture<TestingWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly StorageContext _context;

    public DeviceControllerTest(TestingWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _context = new StorageContext(new DbContextOptionsBuilder<StorageContext>()
            .UseInMemoryDatabase(TestingWebAppFactory.DbName, TestingWebAppFactory.DbRoot).Options);
        _context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        _client.Dispose();
        _context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task TestGetList()
    {
        var storage = new ServerStorage(_context);
        storage.AddDevice(new Device()
        {
            Id = "Device1",
            Token = "DeviceToken1",
            Name = "DeviceName1",
            RegisterDate = new DateTime()
        });
        storage.AddDevice(new Device()
        {
            Id = "Device2",
            Token = "DeviceToken2",
            Name = "DeviceName2",
            RegisterDate = new DateTime()
        });
        storage.AddDevice(new Device()
        {
            Id = "Device3",
            Token = "DeviceToken3",
            Name = "DeviceName3",
            RegisterDate = new DateTime()
        });

        var admin = new DevicesService(_client);
        var deviceList = await admin.GetDevicesList();
        Assert.Equal(3, deviceList.Count());
    }

    [Fact]
    public async Task TestDelete()
    {
        var storage = new ServerStorage(_context);
        var deviceToRemove = new Device()
        {
            Id = "Device1",
            Token = "DeviceToken1",
            Name = "DeviceName1",
            RegisterDate = new DateTime()
        };
        storage.AddDevice(deviceToRemove);
        storage.AddDevice(new Device()
        {
            Id = "Device2",
            Token = "DeviceToken2",
            Name = "DeviceName2",
            RegisterDate = new DateTime()
        });
        storage.AddDevice(new Device()
        {
            Id = "Device3",
            Token = "DeviceToken3",
            Name = "DeviceName3",
            RegisterDate = new DateTime()
        });
        
        var admin = new DevicesService(_client);
        var removeResult = await admin.RemoveDevice(deviceToRemove.Id);
        Assert.True(removeResult);
        var deviceList = await admin.GetDevicesList();
        Assert.Equal(2, deviceList.Count());
        removeResult = await admin.RemoveDevice(deviceToRemove.Id);
        Assert.False(removeResult);
    }

    [Fact]
    public async Task TestRename()
    {
        var storage = new ServerStorage(_context);
        var deviceName1 = "Device1";
        var deviceName2 = "Device2";
        var deviceId1 = "DeviceId1";
        var deviceId2 = "DeviceId2";
        storage.AddDevice(new Device()
        {
            Id = deviceId1,
            Token = "DeviceToken1",
            Name = deviceName1,
            RegisterDate = new DateTime()
        });

        var admin = new DevicesService(_client);
        var deviceList = await admin.GetDevicesList();
        Assert.Equal(deviceName1, deviceList.First().Name);

        var renameResult = await admin.RenameDevice(deviceId1, deviceName2);
        Assert.True(renameResult);
        deviceList = await admin.GetDevicesList();
        Assert.Equal(deviceName2, deviceList.First().Name);
        
        renameResult = await admin.RenameDevice(deviceId2, deviceName2);
        Assert.False(renameResult);
    }
}