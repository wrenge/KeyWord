using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KeyWord.Communication;
using KeyWord.Credentials;
using KeyWord.Crypto;
using KeyWord.Server.Controllers;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace KeyWord.Server.Tests;

[TestFixture]
public class SyncControllerTest
{
    [Test]
    public async Task TestSyncGet()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var contextOptions = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;
        var context = new StorageContext(contextOptions);
        await context.Database.EnsureCreatedAsync();
        var storage = new ServerStorage(context);
        var controller = new SyncController(context, new LoggerFactory().CreateLogger<SyncController>());

        var device1 = new MockDevice();
        var pbkdf2 = new Pbkdf2(1, 16);
        var credentials = new TestCredentials(9);

        device1.DeviceId = Guid.NewGuid().ToString();
        device1.Token = pbkdf2.ComputeKey(new ByteText(device1.DeviceId), new ByteText("Salt")).ToBase64();
        var authId = "DeviceAuthId";
        storage.AddDevice(new Device
        {
            Id = device1.DeviceId,
            Token = device1.Token,
            Name = "Test Sync Send",
            RegisterDate = new DateTime(2022, 1, 1)
        });
        Assert.Greater(storage.GetDevices().Count(), 0);

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
        Assert.Greater(storage.Count, 0);

        var request = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device1.DeviceId, device1.Token).ToBase64(),
            DeviceId = device1.DeviceId,
            LastSyncTime = new DateTime(2022, 1, 3),
            AuthId = authId,
            SyncData = new SyncData
            {
                AddedCredentials = device1.AddedCredentials.ToArray()
            }
        };

        var response = controller.RequestSync(request);
        Assert.IsNotInstanceOf<UnauthorizedObjectResult>(response.Result);
        Assert.NotNull(response.Value);
        Assert.AreEqual(2, response.Value!.SyncData.AddedCredentials.Length);
        Assert.AreEqual(2, response.Value!.SyncData.ModifiedCredentials.Length);
        Assert.AreEqual(2, response.Value!.SyncData.DeletedCredentialsIds.Length);
    }

    [Test]
    public async Task TestSyncSet()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var contextOptions = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;
        var context = new StorageContext(contextOptions);
        await context.Database.EnsureCreatedAsync();
        var storage = new ServerStorage(context);
        var controller = new SyncController(context, new LoggerFactory().CreateLogger<SyncController>());

        var device1 = new MockDevice();
        var pbkdf2 = new Pbkdf2(1, 16);
        var credentials = new TestCredentials(9);

        device1.DeviceId = Guid.NewGuid().ToString();
        device1.Token = pbkdf2.ComputeKey(new ByteText(device1.DeviceId), new ByteText("Salt")).ToBase64();
        storage.AddDevice(new Device
        {
            Id = device1.DeviceId,
            Token = device1.Token,
            Name = "Test Sync Send",
            RegisterDate = new DateTime(2022, 1, 1)
        });
        Assert.Greater(storage.GetDevices().Count(), 0);
        var authId = "DeviceAuthId";

        device1.AddedCredentials.AddRange(new []
        {
            credentials.AddedCredentialsInfos[0],
            credentials.AddedCredentialsInfos[3],
            credentials.AddedCredentialsInfos[6],
        });

        device1.ModifiedCredentials.AddRange(new []
        {
            credentials.ModifiedCredentialsInfos[1],
            credentials.ModifiedCredentialsInfos[4],
            credentials.ModifiedCredentialsInfos[7],
        });

        device1.DeletedCredentialsIds.AddRange(new []
        {
            credentials.RemovedCredentialsInfos[2].Id,
            credentials.RemovedCredentialsInfos[5].Id,
            credentials.RemovedCredentialsInfos[8].Id,
        });

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

        Assert.AreEqual(storedCredentials.Length, storage.GetAddedCredentials(new DateTime(), authId).Count());

        var request = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device1.DeviceId, device1.Token).ToBase64(),
            DeviceId = device1.DeviceId,
            LastSyncTime = new DateTime(2022, 1, 3),
            AuthId = authId,
            SyncData = new SyncData
            {
                AddedCredentials = device1.AddedCredentials.ToArray()
            }
        };

        var response = controller.RequestSync(request);
        Assert.IsNotInstanceOf<UnauthorizedObjectResult>(response.Result);
        Assert.NotNull(response.Value);
        Assert.AreEqual(storedCredentials.Length + request.SyncData.AddedCredentials.Length,
            storage.GetAddedCredentials(new DateTime(), authId).Count());

        request.SyncData.AddedCredentials = Array.Empty<ClassicCredentialsInfo>();
        request.SyncData.ModifiedCredentials = device1.ModifiedCredentials.ToArray();

        response = controller.RequestSync(request);
        Assert.NotNull(response.Value);
        var modifiedInStorage =
            storage.GetModifiedCredentials(credentials.AddedCredentialsInfos[0].CreationTime, authId);
        Assert.AreEqual(device1.ModifiedCredentials.Count,
            modifiedInStorage.Intersect(device1.ModifiedCredentials).Count());

        request.SyncData.ModifiedCredentials = Array.Empty<ClassicCredentialsInfo>();
        request.SyncData.DeletedCredentialsIds = device1.DeletedCredentialsIds.ToArray();

        response = controller.RequestSync(request);
        Assert.NotNull(response.Value);
        var removedIdsInStorage =
            storage.GetDeletedCredentials(credentials.AddedCredentialsInfos[0].CreationTime, authId);
        var removedInStorage = storage.GetAllCredentials(authId).Where(x => x.RemoveTime != null);
        Assert.AreEqual(device1.DeletedCredentialsIds.Count, removedIdsInStorage.Count());
        Assert.AreEqual(1, removedInStorage.DistinctBy(x => (x.Identifier, x.Login, x.Password)).Count());
    }

    [Test]
    public async Task TestWrongDevice()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var contextOptions = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;
        var context = new StorageContext(contextOptions);
        await context.Database.EnsureCreatedAsync();
        var storage = new ServerStorage(context);
        var controller = new SyncController(context, new LoggerFactory().CreateLogger<SyncController>());

        var device1 = new MockDevice();
        var device2 = new MockDevice();
        var pbkdf2 = new Pbkdf2(1, 16);

        device1.DeviceId = Guid.NewGuid().ToString();
        device1.Token = pbkdf2.ComputeKey(new ByteText(device1.DeviceId), new ByteText("Salt")).ToBase64();
        device2.DeviceId = Guid.NewGuid().ToString();
        device2.Token = pbkdf2.ComputeKey(new ByteText(device2.DeviceId), new ByteText("Salt")).ToBase64();

        storage.AddDevice(new Device
        {
            Id = device1.DeviceId,
            Token = device1.Token,
            Name = "Test Sync Send",
            RegisterDate = new DateTime(2022, 1, 1)
        });
        Assert.Greater(storage.GetDevices().Count(), 0);

        var request = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device1.DeviceId, device1.Token).ToBase64(),
            DeviceId = device1.DeviceId,
            LastSyncTime = new DateTime(2022, 1, 3),
            AuthId = "AuthKey",
            SyncData = new SyncData()
        };

        var response = controller.RequestSync(request);
        Assert.IsNotInstanceOf<UnauthorizedObjectResult>(response.Result);

        request.DeviceId = device2.DeviceId;
        response = controller.RequestSync(request);
        Assert.IsInstanceOf<UnauthorizedObjectResult>(response.Result);
    }

    [Test]
    public async Task TestWrongToken()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var contextOptions = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;
        var context = new StorageContext(contextOptions);
        await context.Database.EnsureCreatedAsync();
        var storage = new ServerStorage(context);
        var controller = new SyncController(context, new LoggerFactory().CreateLogger<SyncController>());

        var device1 = new MockDevice();
        var device2 = new MockDevice();
        var pbkdf2 = new Pbkdf2(1, 16);

        device1.DeviceId = Guid.NewGuid().ToString();
        device1.Token = pbkdf2.ComputeKey(new ByteText(device1.DeviceId), new ByteText("Salt")).ToBase64();
        device2.DeviceId = Guid.NewGuid().ToString();
        device2.Token = pbkdf2.ComputeKey(new ByteText(device2.DeviceId), new ByteText("Salt")).ToBase64();

        storage.AddDevice(new Device
        {
            Id = device1.DeviceId,
            Token = device1.Token,
            Name = "Test Sync Send",
            RegisterDate = new DateTime(2022, 1, 1)
        });
        Assert.Greater(storage.GetDevices().Count(), 0);

        var request = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device1.DeviceId, device1.Token).ToBase64(),
            DeviceId = device1.DeviceId,
            LastSyncTime = new DateTime(2022, 1, 3),
            AuthId = "AuthKey",
            SyncData = new SyncData()
        };

        var response = controller.RequestSync(request);
        Assert.IsNotInstanceOf<UnauthorizedObjectResult>(response.Result);

        request.AuthKey = SyncUtilities.GetDeviceAuthKey(device2.DeviceId, device2.Token).ToBase64();
        response = controller.RequestSync(request);
        Assert.IsInstanceOf<UnauthorizedObjectResult>(response.Result);
    }

    [Test]
    public async Task TestAuthId()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var contextOptions = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;
        var context = new StorageContext(contextOptions);
        await context.Database.EnsureCreatedAsync();
        var storage = new ServerStorage(context);
        var controller = new SyncController(context, new LoggerFactory().CreateLogger<SyncController>());

        var pbkdf2 = new Pbkdf2(1, 16);
        var credentials = new TestCredentials(9);

        var device1 = new MockDevice
        {
            DeviceId = Guid.NewGuid().ToString()
        };
        device1.Token = pbkdf2.ComputeKey(new ByteText(device1.DeviceId), new ByteText("Salt")).ToBase64();
        var authId1 = "Device1AuthId";

        var device2 = new MockDevice
        {
            DeviceId = Guid.NewGuid().ToString()
        };
        device2.Token = pbkdf2.ComputeKey(new ByteText(device2.DeviceId), new ByteText("Salt")).ToBase64();
        var authId2 = "Device2AuthId";

        storage.AddDevice(new Device
        {
            Id = device1.DeviceId,
            Token = device1.Token,
            Name = "TestDevice1",
            RegisterDate = new DateTime(2022, 1, 1)
        });
        storage.AddDevice(new Device
        {
            Id = device2.DeviceId,
            Token = device2.Token,
            Name = "TestDevice2",
            RegisterDate = new DateTime(2022, 1, 1)
        });
        Assert.AreEqual(2, storage.GetDevices().Count());
        
        storage.AddCredentials(new []
        {
            credentials.AddedCredentialsInfos[0],
            credentials.AddedCredentialsInfos[1]
        }, authId1);
        
        storage.AddCredentials(new []
        {
            credentials.AddedCredentialsInfos[2],
        }, authId2);
        
        var request1 = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device1.DeviceId, device1.Token).ToBase64(),
            DeviceId = device1.DeviceId,
            AuthId = authId1
        };

        var response1 = controller.RequestSync(request1);
        Assert.NotNull(response1.Value);
        Assert.AreEqual(2, response1.Value!.SyncData.AddedCredentials.Length);
        
        var request2 = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device2.DeviceId, device2.Token).ToBase64(),
            DeviceId = device2.DeviceId,
            AuthId = authId2
        };

        var response2 = controller.RequestSync(request2);
        Assert.NotNull(response1.Value);
        Assert.AreEqual(1, response2.Value!.SyncData.AddedCredentials.Length);
    }
}