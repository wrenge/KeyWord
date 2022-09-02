using KeyWord.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeyWord.Server.Storage;

namespace KeyWord.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SyncController : ControllerBase
{
    public ServerStorage Storage { get; }
    private readonly ILogger<SyncController> _logger;

    public SyncController(StorageContext context, ILogger<SyncController> logger)
    {
        Storage = new ServerStorage(context);
        _logger = logger;
    }

    [HttpPost(nameof(RequestSync))]
    public ActionResult<SyncResponse?> RequestSync([FromBody] SyncRequest request)
    {
        var device = Storage.FindDeviceById(request.DeviceId);
        if (device == null)
            return Unauthorized("Device is not registered");

        var keyValid = request.AuthKey == SyncUtilities.GetDeviceAuthKey(device.Id, device.Token).ToBase64();
        
        if(!keyValid)
            return Unauthorized("Auth key is not valid");

        var authId = request.AuthId;
        if (string.IsNullOrEmpty(authId))
            return Unauthorized("AuthId is empty");
        
        var syncData = new SyncData
        {
            AddedCredentials = Storage.GetAddedCredentials(request.LastSyncTime, authId).ToArray(),
            ModifiedCredentials = Storage.GetModifiedCredentials(request.LastSyncTime, authId).ToArray(),
            DeletedCredentialsIds = Storage.GetDeletedCredentials(request.LastSyncTime, authId).ToArray()
        };
        
        Storage.AddCredentials(request.SyncData.AddedCredentials, authId);
        Storage.UpdateCredentials(request.SyncData.ModifiedCredentials, authId);
        Storage.DeleteCredentials(request.SyncData.DeletedCredentialsIds, authId);

        var result = new SyncResponse();
        result.SyncData = syncData;
        
        return result;
    }
}