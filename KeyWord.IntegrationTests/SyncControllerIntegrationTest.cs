using System.Collections.Immutable;
using KeyWord.Client.Network;
using KeyWord.Communication;
using KeyWord.Credentials;
using KeyWord.Server.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KeyWord.IntegrationTests;

public class SyncControllerIntegrationTest : IClassFixture<TestingWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly StorageContext _context;

    public SyncControllerIntegrationTest(TestingWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _context = new StorageContext(new DbContextOptionsBuilder<StorageContext>()
            .UseInMemoryDatabase(TestingWebAppFactory.DbName, TestingWebAppFactory.DbRoot).Options);
        _context.Database.EnsureCreated();
    }
    
    [Fact]
    public async Task TestSyncGet()
    {
        var storage = new ServerStorage(_context);
        
        var device = new Device()
        {
            Id = "MockDeviceId",
            Token = "MockDeviceToken",
            Name = "MockDeviceName",
            RegisterDate = new DateTime()
        };
        storage.AddDevice(device);
        var authId = "AuthId";

        var credentials = new TestCredentials(9);
        var storageInfos = new []
        {
            credentials.AddedCredentialsInfos[0],
            credentials.AddedCredentialsInfos[3],
            credentials.AddedCredentialsInfos[6],
            credentials.ModifiedCredentialsInfos[1],
            credentials.ModifiedCredentialsInfos[4],
            credentials.ModifiedCredentialsInfos[7],
            credentials.RemovedCredentialsInfos[2],
            credentials.RemovedCredentialsInfos[5],
            credentials.RemovedCredentialsInfos[8],
        };
        storage.AddCredentials(storageInfos, authId);
        
        var request = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device.Id, device.Token).ToBase64(),
            DeviceId = device.Id,
            AuthId = authId,
            LastSyncTime = new DateTime(2022, 1, 3),
            SyncData = new SyncData()
        };

        var clientService = new SynchronizationService(_client);
        var syncResponse = await clientService.TrySync(device.Id, device.Token, authId, request.LastSyncTime,
            request.SyncData.AddedCredentials, 
            request.SyncData.ModifiedCredentials,
            request.SyncData.DeletedCredentialsIds);

        Assert.NotNull(syncResponse);
        Assert.Equal(2, syncResponse!.SyncData.AddedCredentials.Length);
        Assert.Equal(2, syncResponse!.SyncData.ModifiedCredentials.Length);
        Assert.Equal(2, syncResponse!.SyncData.DeletedCredentialsIds.Length);
    }
    
    [Fact]
    public async Task TestSyncSet()
    {
        var storage = new ServerStorage(_context);
        
        var device = new Device()
        {
            Id = "MockDeviceId",
            Token = "MockDeviceToken",
            Name = "MockDeviceName",
            RegisterDate = new DateTime()
        };
        storage.AddDevice(device);
        
        var credentials = new TestCredentials(9);
        var authId = "AuthId";
        
        var storedCredentials = new []
        {
            credentials.AddedCredentialsInfos[1],
            credentials.AddedCredentialsInfos[2],
            credentials.AddedCredentialsInfos[4],
            credentials.AddedCredentialsInfos[5],
            credentials.AddedCredentialsInfos[7],
            credentials.AddedCredentialsInfos[8],
        };
        storage.AddCredentials(storedCredentials, authId);

        var request = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device.Id, device.Token).ToBase64(),
            DeviceId = device.Id,
            LastSyncTime = new DateTime(2022, 1, 3),
            AuthId = authId,
            SyncData = new SyncData
            {
                AddedCredentials = new[]
                {
                    credentials.AddedCredentialsInfos[0],
                    credentials.AddedCredentialsInfos[3],
                    credentials.AddedCredentialsInfos[6]
                }
            }
        };

        var clientService = new SynchronizationService(_client);
        var syncResponse = await clientService.TrySync(device.Id, device.Token, authId, request.LastSyncTime,
            request.SyncData.AddedCredentials, 
            request.SyncData.ModifiedCredentials,
            request.SyncData.DeletedCredentialsIds);

        Assert.NotNull(syncResponse);
        Assert.Equal(storedCredentials.Length + request.SyncData.AddedCredentials.Length, 
            storage.GetAddedCredentials(new DateTime(), authId).Count());

        request.SyncData.AddedCredentials = Array.Empty<ClassicCredentialsInfo>();
        request.SyncData.ModifiedCredentials = new []
        {
            credentials.ModifiedCredentialsInfos[1],
            credentials.ModifiedCredentialsInfos[4],
            credentials.ModifiedCredentialsInfos[7]
        };

        syncResponse = await clientService.TrySync(device.Id, device.Token, authId, request.LastSyncTime,
            request.SyncData.AddedCredentials, 
            request.SyncData.ModifiedCredentials,
            request.SyncData.DeletedCredentialsIds);
        
        Assert.NotNull(syncResponse);
        await ReloadTable(_context.ClassicCredentialsInfos); // Костыль, чтобы обновить кеши
        var modifiedInStorage = storage.GetModifiedCredentials(credentials.AddedCredentialsInfos[0].CreationTime, authId);
        Assert.Equal(request.SyncData.ModifiedCredentials.Length, modifiedInStorage.Intersect(request.SyncData.ModifiedCredentials).Count());

        request.SyncData.ModifiedCredentials = Array.Empty<ClassicCredentialsInfo>();
        request.SyncData.DeletedCredentialsIds = new []
        {
            credentials.RemovedCredentialsInfos[2].Id,
            credentials.RemovedCredentialsInfos[5].Id,
            credentials.RemovedCredentialsInfos[8].Id
        };
        
        syncResponse = await clientService.TrySync(device.Id, device.Token, authId, request.LastSyncTime,
            request.SyncData.AddedCredentials, 
            request.SyncData.ModifiedCredentials,
            request.SyncData.DeletedCredentialsIds);

        Assert.NotNull(syncResponse);
        await ReloadTable(_context.ClassicCredentialsInfos); // Костыль, чтобы обновить кеши
        var removedIdsInStorage = storage.GetDeletedCredentials(credentials.AddedCredentialsInfos[0].CreationTime, authId);
        var removedInStorage = storage.GetAllCredentials(authId).Where(x => x.RemoveTime != null);
        Assert.Equal(request.SyncData.DeletedCredentialsIds.Length, removedIdsInStorage.Count());
        Assert.Single(removedInStorage.DistinctBy(x => (x.Identifier, x.Login, x.Password)));
    }

    private async Task ReloadTable<T>(IEnumerable<T> table)
    {
        foreach (var i in table)
        {
            await _context.Entry(i!).ReloadAsync();
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}