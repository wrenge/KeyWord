using KeyWord.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeyWord.Server.Storage;

namespace KeyWord.Server.Controllers;

// [Authorize]
[ApiController]
[Route("[controller]")]
public class SyncController : ControllerBase
{
    private readonly ILogger<SyncController> _logger;
    private readonly ServerStorage _storage;

    public SyncController(ILogger<SyncController> logger, string dbFilePath = "keyword.db3")
    {
        _logger = logger;
        _storage = new ServerStorage(dbFilePath); // TODO: move to appsettings.json
    }

    [HttpPost(Name = nameof(RequestSync))]
    public ActionResult<SyncResponse?> RequestSync([FromBody] SyncRequest request)
    {
        var device = _storage.FindDeviceById(request.DeviceId);
        if (device == null)
            return Unauthorized(null);

        var keyValid = request.AuthKey == SyncUtilities.GetDeviceAuthKey(device.Id, device.Token).ToBase64();
        
        if(!keyValid)
            return Unauthorized(null);
        
        var syncData = new SyncData
        {
            AddedCredentials = _storage.GetAddedCredentials(request.LastSyncTime).ToArray(),
            ModifiedCredentials = _storage.GetModifiedCredentials(request.LastSyncTime).ToArray(),
            DeletedCredentialsIds = _storage.GetDeletedCredentials(request.LastSyncTime).ToArray()
        };
        
        _storage.AddCredentials(request.SyncData.AddedCredentials);
        _storage.UpdateCredentials(request.SyncData.ModifiedCredentials);
        _storage.DeleteCredentials(request.SyncData.DeletedCredentialsIds);

        var result = new SyncResponse();
        result.SyncData = syncData;
        
        return result;
    }
}