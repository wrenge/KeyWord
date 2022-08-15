using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KeyWord.Server.Controllers;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace KeyWord.Server.Tests;

[TestFixture]
public class RegisterControllerTest
{
    [SetUp]
    public void Setup()
    {
        var dbPath = GetDbPath();
        if (File.Exists(dbPath))
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
        Assert.AreEqual(1, storage.GetDevices().Count());
    }

    [Test]
    public async Task TestRegistrationDeny()
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
                controller.DenyPendingDevice();
            })
        );

        Assert.IsAssignableFrom<ForbidResult>(approvalResult);
        Assert.LessOrEqual(0, storage.GetDevices().Count());
    }

    [Test]
    public void TestRegistrationTokenNotFound()
    {
        var dbFilePath = GetDbPath();
        var storage = new ServerStorage(dbFilePath);
        var controller = new RegisterController(storage, new LoggerFactory().CreateLogger<RegisterController>());

        var token = controller.RequestNewToken();
        Assert.IsAssignableFrom<NotFoundResult>(token.Result);
    }

    [Test]
    public async Task TestRequestDeviceCandidate()
    {
        var dbFilePath = GetDbPath();
        var storage = new ServerStorage(dbFilePath);
        var controller = new RegisterController(storage, new LoggerFactory().CreateLogger<RegisterController>());
        var deviceResp = await controller.RequestDeviceCandidate();
        Assert.IsAssignableFrom<ObjectResult>(deviceResp.Result);
        Assert.AreEqual(((ObjectResult)deviceResp.Result!).StatusCode, StatusCodes.Status400BadRequest);
        controller.StartNewRegistration();
        await Task.WhenAll(
            Task.Run(async () => deviceResp = await controller.RequestDeviceCandidate()),
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                controller.StartNewRegistration();
            })
        );
        Assert.IsAssignableFrom<StatusCodeResult>(deviceResp.Result);
        Assert.AreEqual(((StatusCodeResult)deviceResp.Result!).StatusCode, StatusCodes.Status503ServiceUnavailable);
    }
    
    [Test]
    public void TestApproveDenyFails()
    {
        var dbFilePath = GetDbPath();
        var storage = new ServerStorage(dbFilePath);
        var controller = new RegisterController(storage, new LoggerFactory().CreateLogger<RegisterController>());
        var resp = controller.ApprovePendingDevice();
        Assert.IsAssignableFrom<ObjectResult>(resp);
        Assert.AreEqual(((ObjectResult) resp).StatusCode, StatusCodes.Status400BadRequest);
        resp = controller.DenyPendingDevice();
        Assert.IsAssignableFrom<ObjectResult>(resp);
        Assert.AreEqual(((ObjectResult) resp).StatusCode, StatusCodes.Status400BadRequest);
    }

    [Test]
    public void TestPostDeviceFails()
    {
        var dbFilePath = GetDbPath();
        var storage = new ServerStorage(dbFilePath);
        var controller = new RegisterController(storage, new LoggerFactory().CreateLogger<RegisterController>());
        
        var mockDevice = new Device()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "MockDevice1",
            RegisterDate = new DateTime()
        };
        
        // No session
        var resp = controller.PostDeviceInfo(mockDevice);
        Assert.IsAssignableFrom<ObjectResult>(resp);
        Assert.AreEqual(((ObjectResult) resp).StatusCode, StatusCodes.Status400BadRequest);

        controller.StartNewRegistration();
        var token = controller.RequestNewToken();
        
        // Wrong token
        resp = controller.PostDeviceInfo(mockDevice);
        Assert.IsAssignableFrom<ObjectResult>(resp);
        Assert.AreEqual(((ObjectResult) resp).StatusCode, StatusCodes.Status400BadRequest);

        mockDevice.Token = token.Value!.Token;
        
        // Ok
        resp = controller.PostDeviceInfo(mockDevice);
        Assert.IsAssignableFrom<OkResult>(resp);
        
        // Session occupied
        resp = controller.PostDeviceInfo(mockDevice);
        Assert.IsAssignableFrom<ObjectResult>(resp);
        Assert.AreEqual(((ObjectResult) resp).StatusCode, StatusCodes.Status400BadRequest);
    }
    
    [Test]
    public async Task TestDeviceApprovalCancel()
    {
        var dbFilePath = GetDbPath();
        var storage = new ServerStorage(dbFilePath);
        var controller = new RegisterController(storage, new LoggerFactory().CreateLogger<RegisterController>());
        
        controller.StartNewRegistration();
        var token = controller.RequestNewToken();
        
        var mockDevice = new Device()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "MockDevice1",
            RegisterDate = new DateTime(),
            Token = token.Value!.Token
        };

        // Ok
        var resp = controller.PostDeviceInfo(mockDevice);
        Assert.IsAssignableFrom<OkResult>(resp);

        ActionResult? approvalResp = null;
        await Task.WhenAll(
            Task.Run(async () => approvalResp = await controller.GetDeviceApproval(mockDevice)),
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                controller.StartNewRegistration();
            })
        );
        
        Assert.IsAssignableFrom<ForbidResult>(approvalResp);
    }
}