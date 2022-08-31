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
    public ServerStorage Storage { get; }
    private readonly ILogger<SyncController> _logger;

    public SyncController(StorageContext context, ILogger<SyncController> logger)
    {
        Storage = new ServerStorage(context);
        _logger = logger;
    }

    [HttpPost(Name = nameof(RequestSync))]
    public ActionResult<SyncResponse?> RequestSync([FromBody] SyncRequest request)
    {
        var device = Storage.FindDeviceById(request.DeviceId);
        if (device == null)
            return Unauthorized(null);

        var keyValid = request.AuthKey == SyncUtilities.GetDeviceAuthKey(device.Id, device.Token).ToBase64();
        
        if(!keyValid)
            return Unauthorized(null);
        
        var syncData = new SyncData
        {
            AddedCredentials = Storage.GetAddedCredentials(request.LastSyncTime).ToArray(),
            ModifiedCredentials = Storage.GetModifiedCredentials(request.LastSyncTime).ToArray(),
            DeletedCredentialsIds = Storage.GetDeletedCredentials(request.LastSyncTime).ToArray()
        };
        
        Storage.AddCredentials(request.SyncData.AddedCredentials);
        Storage.UpdateCredentials(request.SyncData.ModifiedCredentials);
        Storage.DeleteCredentials(request.SyncData.DeletedCredentialsIds);

        var result = new SyncResponse();
        result.SyncData = syncData;
        
        return result;
    }
}