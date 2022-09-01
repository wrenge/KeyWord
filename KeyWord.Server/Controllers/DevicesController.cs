using System.Text;
using System.Web;
using KeyWord.Communication;
using KeyWord.Server.Storage;
using Microsoft.AspNetCore.Mvc;

namespace KeyWord.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DevicesController : ControllerBase
{
    public IStorage Storage { get; }
    private readonly ILogger<DevicesController> _logger;
    
    public DevicesController (IStorage storage, ILogger<DevicesController> logger)
    {
        Storage = storage;
        _logger = logger;
    }
    
    [HttpGet(nameof(GetDeviceList))]
    public ActionResult<Device[]> GetDeviceList()
    {
        return Storage.GetDevices().ToArray();
    }

    [HttpDelete(nameof(DeleteDevice) + "/{idUrlEncoded}")]
    public ActionResult DeleteDevice(string idUrlEncoded)
    {
        var id = HttpUtility.UrlDecode(idUrlEncoded);
        var result = Storage.DeleteDevice(id);
        return result ? Ok() : NotFound();
    }
    
    [HttpPut(nameof(RenameDevice))]
    public ActionResult RenameDevice([FromBody] RenameDeviceRequestData data)
    {
        var result = Storage.RenameDevice(data.Id, data.Name);
        return result ? Ok() : NotFound();
    }
}