using System;
using System.IO;
using System.Linq;
using KeyWord.Communication;
using KeyWord.Credentials;
using KeyWord.Crypto;
using KeyWord.Server.Controllers;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace KeyWord.Server.Tests;

[TestFixture]
public class SyncControllerTest
{
    [SetUp]
    public void Setup()
    {
        var dbPath = GetDbPath();
        if(File.Exists(dbPath))
            File.Delete(dbPath);
    }
    
    [TearDown]
    public void TearDown()
    {
        // Hacks to force SQLite release a file
        GC.Collect();
        GC.WaitForPendingFinalizers();
        SqliteConnection.ClearAllPools();

        var dbPath = GetDbPath();
        if (File.Exists(dbPath))
            File.Delete(dbPath);
    }
    
    private static string GetDbPath()
    {
        var currentContext = TestContext.CurrentContext;
        var dbPath = $"{currentContext.Test.ID}.db3";
        return Path.Combine(currentContext.TestDirectory, dbPath);
    }
    
    [Test]
    public void TestSyncGet()
    {
        var dbFilePath = GetDbPath();
        var storage = new ServerStorage(dbFilePath);
        var controller = new SyncController(null!, dbFilePath);
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
        storage.AddCredentials(storageInfos);
        Assert.Greater(storage.Count, 0);
        
        var request = new SyncRequest
        {
            AuthKey = SyncUtilities.GetDeviceAuthKey(device1.DeviceId, device1.Token).ToBase64(),
            DeviceId = device1.DeviceId,
            LastSyncTime = new DateTime(2022, 1, 3),
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
    
    // TODO: Test send
    // TODO: Test wrong token
    // TODO: Test wrong device
}