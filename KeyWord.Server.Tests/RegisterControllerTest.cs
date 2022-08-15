using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KeyWord.Server.Controllers;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace KeyWord.Server.Tests;

[TestFixture]
public class RegisterControllerTest
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
    public async Task TestRegistration()
    {
        var dbFilePath = GetDbPath();
        var storage = new ServerStorage(dbFilePath);
        var controller = new RegisterController(storage, new LoggerFactory().CreateLogger<RegisterController>());
        Assert.LessOrEqual(0, storage.GetDevices().Count());

        controller.StartNewRegistration();
        var token = controller.RequestNewToken();
        Assert.NotNull(token.Value);
        
        var mockDevice = new Device()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "MockDevice1",
            RegisterDate = new DateTime(),
            Token = token.Value!.Token
        };

        ActionResult<Device?>? device = null;
        await Task.WhenAll(
            Task.Run(async () => device = await controller.RequestDeviceCandidate()),
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                controller.PostDeviceInfo(mockDevice);
            })
        );
        
        Assert.NotNull(device);
        Assert.NotNull(device!.Value);

        ActionResult? approvalResult = null;
        await Task.WhenAll(
            Task.Run(async () => approvalResult = await controller.GetDeviceApproval(mockDevice)),
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                controller.ApprovePendingDevice();
            })
        );
        
        Assert.IsAssignableFrom<OkResult>(approvalResult);
    }
}