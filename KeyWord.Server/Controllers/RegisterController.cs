using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using KeyWord.Communication;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Mvc;

namespace KeyWord.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RegisterController : ControllerBase
{
    private const int TokenByteLength = 32;
    private const int TokenTimeout = 30;
    private readonly ServerStorage _storage;
    private readonly ILogger<RegisterController> _logger;
    private RegistrationSession? _currentSession;

    public RegisterController(ServerStorage storage, ILogger<RegisterController> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    // Only from admin client
    [Host("localhost")]
    [HttpPost(nameof(StartNewRegistration))]
    public ActionResult StartNewRegistration()
    {
        if (_currentSession is {IsClosed: false, IsExpired: false })
        {
            _logger.LogWarning( "Closing session early: {Token}", _currentSession.Token);
            _currentSession.Close();
        }
        
        var newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenByteLength));
        _currentSession = new RegistrationSession(newToken, DateTime.Now, TimeSpan.FromSeconds(TokenTimeout));
        _logger.LogInformation( "Starting session {Token}", _currentSession.Token);
        CloseSessionOnExpire(_currentSession);
        return Ok();
    }

    private async void CloseSessionOnExpire(RegistrationSession session)
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
        if (_currentSession == null || _currentSession.IsExpired || _currentSession.IsClosed)
        {
            return NotFound();
        }
        return new RegisterInfo(_currentSession.Token, _currentSession.GetExpireDate(), GetLocalIpAddress());
    }

    private static IPAddress GetLocalIpAddress()
    {
        var hostname = Environment.MachineName;
        var host = Dns.GetHostEntry(hostname);
        return host.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
    }

    // Only from admin client
    [Host("localhost")]
    [HttpGet(nameof(RequestDeviceCandidate))]
    public async Task<ActionResult<DeviceCandidate?>> RequestDeviceCandidate()
    {
        if (_currentSession == null || _currentSession.IsClosed)
        {
            const string errorMsg = "Tried to await device without starting a session";
            _logger.LogError(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        if (_currentSession.IsExpired)
        {
            const string errorMsg = "Tried to await device with expired session";
            _logger.LogError(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        try
        {
            var device = await _currentSession.DeviceCandidate.Task;
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
        if (_currentSession == null || _currentSession.IsClosed)
        {
            const string errorMsg = "Tried to approve device without session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        if (!_currentSession.DeviceCandidate.Task.IsCompleted)
        {
            const string errorMsg = "Tried to approve device without device itself";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (_currentSession.IsExpired)
        {
            const string errorMsg = "Tried to approve device with an expired session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        _currentSession.IsDeviceApproved = true;
        _currentSession.Close();
        var deviceCandidate = _currentSession.DeviceCandidate.Task.Result;
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
        if (_currentSession == null || _currentSession.IsClosed)
        {
            const string errorMsg = "Tried to deny device without session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (_currentSession.IsExpired)
        {
            const string errorMsg = "Tried to deny device with an expired session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        _currentSession.IsDeviceApproved = false;
        _currentSession.Close();
        return Ok();
    }
    
    // Only from app client
    [HttpPost(nameof(PostDeviceInfo))]
    public ActionResult PostDeviceInfo([FromBody] DeviceCandidate device)
    {
        if (_currentSession == null || _currentSession.IsClosed)
        {
            const string errorMsg = "Tried to post device info without session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        if (device.Token != _currentSession.Token)
        {
            const string errorMsg = "Tried to post device info with an invalid token";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (_currentSession.IsExpired)
        {
            const string errorMsg = "Tried to post device info with an expired session";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }
        
        if (_currentSession.IsOccupied)
        {
            const string errorMsg = "Tried to post device info with an occupied token";
            _logger.LogInformation(errorMsg);
            return Problem(errorMsg, statusCode: StatusCodes.Status400BadRequest);
        }

        _currentSession.IsOccupied = true;
        _currentSession.DeviceCandidate.SetResult(device);
        return Ok();
    }
    
    // Only from app client
    [HttpGet(nameof(GetDeviceApproval) + "/{deviceId:string}")]
    public async Task<ActionResult> GetDeviceApproval(string deviceId)
    {
        if (_currentSession == null
            || _currentSession.IsClosed
            || _currentSession.IsExpired
            )
        {
            var existingDevice = _storage.FindDeviceById(deviceId);
            if (existingDevice != null)
            {
                return Ok();
            }

            return Forbid();
        }

        try
        {
            await _currentSession.GetApprovalHandle();
            return Ok();
        }
        catch (TaskCanceledException)
        {
            return Forbid();
        }
    }
}