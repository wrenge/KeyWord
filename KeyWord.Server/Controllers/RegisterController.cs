using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using KeyWord.Communication;
using KeyWord.Server.Services;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Mvc;

namespace KeyWord.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RegisterController : ControllerBase
{
    private const int TokenByteLength = 32;
    // private const int TokenTimeout = 30;
    private const int TokenTimeout = 600;
    private readonly RegisterService _registerService;
    private readonly ILogger<RegisterController> _logger;
    private readonly IStorage _storage;
    private RegisterSession? CurrentSession
    {
        get => _registerService.CurrentSession;
        set => _registerService.CurrentSession = value;
    }

    public RegisterController(IStorage storage, RegisterService registerService, ILogger<RegisterController> logger)
    {
        _storage = storage;
        _logger = logger;
        _registerService = registerService;
    }

    // Only from admin client
    [Host("localhost")]
    [HttpPost(nameof(StartNewRegistration))]
    public ActionResult StartNewRegistration()
    {
        if (CurrentSession is {IsClosed: false, IsExpired: false })
        {
            _logger.LogWarning( "Closing session early: {Token}", CurrentSession.Token);
            CurrentSession.Close();
        }
        
        var newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenByteLength));
        var port = NetworkConstants.RequestPort; // TODO генерировать рандомный порт
        CurrentSession = new RegisterSession(newToken, DateTime.Now, TimeSpan.FromSeconds(TokenTimeout), port);
        _logger.LogInformation( "Starting session {Token}", CurrentSession.Token);
        Task.Run(async () => await CurrentSession.ListenDiscoveryAsync());
        Task.Run(async () => await CloseSessionOnExpire(CurrentSession));
        return Ok();
    }

    private async Task CloseSessionOnExpire(RegisterSession session)
    {
        await Task.Delay(session.GetTimeLeft());
        _logger.LogInformation( "Closing session {Token}", session.Token);
        session.Close();
    }

    // Only from admin client
    [Host("localhost")]
    [HttpGet(nameof(RequestNewToken))]
    public ActionResult<RegisterInfo> RequestNewToken()
    {
        if (CurrentSession == null || CurrentSession.IsExpired || CurrentSession.IsClosed)
        {
            return NotFound();
        }
        return new RegisterInfo(CurrentSession.Token, CurrentSession.GetExpireDate(), CurrentSession.Port);
    }

    // Only from admin client
    [Host("localhost")]
    [HttpGet(nameof(RequestDeviceCandidate))]
    public async Task<ActionResult<DeviceCandidate?>> RequestDeviceCandidate()
    {
        if (CurrentSession == null || CurrentSession.IsClosed)
        {
            const string errorMsg = "Tried to await device without starting a session";
            _logger.LogError(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        if (CurrentSession.IsExpired)
        {
            const string errorMsg = "Tried to await device with expired session";
            _logger.LogError(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        try
        {
            var device = await CurrentSession.DeviceCandidate.Task;
            // TODO: if connection interrupted then close session
            return device;
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    // Only from admin client
    [Host("localhost")]
    [HttpPost(nameof(ApprovePendingDevice))]
    public ActionResult ApprovePendingDevice()
    {
        if (CurrentSession == null || CurrentSession.IsClosed)
        {
            const string errorMsg = "Tried to approve device without session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        if (!CurrentSession.DeviceCandidate.Task.IsCompleted)
        {
            const string errorMsg = "Tried to approve device without device itself";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (CurrentSession.IsExpired)
        {
            const string errorMsg = "Tried to approve device with an expired session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        CurrentSession.IsDeviceApproved = true;
        CurrentSession.Close();
        var deviceCandidate = CurrentSession.DeviceCandidate.Task.Result;
        _storage.AddDevice(new Device()
        {
            Id = deviceCandidate.Id,
            Name = deviceCandidate.Name,
            RegisterDate = DateTime.Now,
            Token = deviceCandidate.Token
        });
        return Ok();
    }
    
    // Only from admin client
    [Host("localhost")]
    [HttpPost(nameof(DenyPendingDevice))]
    public ActionResult DenyPendingDevice()
    {
        if (CurrentSession == null || CurrentSession.IsClosed)
        {
            const string errorMsg = "Tried to deny device without session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (CurrentSession.IsExpired)
        {
            const string errorMsg = "Tried to deny device with an expired session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        CurrentSession.IsDeviceApproved = false;
        CurrentSession.Close();
        return Ok();
    }
    
    // Only from app client
    [HttpPost(nameof(PostDeviceInfo))]
    public ActionResult PostDeviceInfo([FromBody] DeviceCandidate device)
    {
        if (CurrentSession == null || CurrentSession.IsClosed)
        {
            const string errorMsg = "Tried to post device info without session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        if (device.Token != CurrentSession.Token)
        {
            const string errorMsg = "Tried to post device info with an invalid token";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (CurrentSession.IsExpired)
        {
            const string errorMsg = "Tried to post device info with an expired session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (CurrentSession.IsOccupied)
        {
            const string errorMsg = "Tried to post device info with an occupied token";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        CurrentSession.IsOccupied = true;
        CurrentSession.DeviceCandidate.SetResult(device);
        return Ok();
    }
    
    // Only from app client
    [HttpGet(nameof(GetDeviceApproval) + "/{deviceId:alpha}")]
    public async Task<ActionResult> GetDeviceApproval(string deviceId)
    {
        var existingDevice = _storage.FindDeviceById(deviceId);
        if (existingDevice != null)
        {
            return Ok();
        }
        
        if (CurrentSession == null
            || CurrentSession.IsClosed
            || CurrentSession.IsExpired
            || !CurrentSession.DeviceCandidate.Task.IsCompleted
            || CurrentSession.DeviceCandidate.Task.Result.Id != deviceId
            )
        {
            return Forbid();
        }

        try
        {
            await CurrentSession.GetApprovalHandle();
            return Ok();
        }
        catch (TaskCanceledException)
        {
            return Forbid();
        }
    }
}