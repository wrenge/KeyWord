using KeyWord.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyWord.Server.Controllers;

// [Authorize]
[ApiController]
[Route("[controller]")]
public class SyncController : ControllerBase
{
    private readonly ILogger<SyncController> _logger;

    public SyncController(ILogger<SyncController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = nameof(RequestSync))]
    public SyncResponse RequestSync()
    {
        // TODO
        return new SyncResponse();
    }
}