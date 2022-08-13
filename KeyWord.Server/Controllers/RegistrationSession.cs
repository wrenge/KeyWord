using KeyWord.Server.Storage;

namespace KeyWord.Server.Controllers;

public class RegistrationSession
{
    public string Token { get; }
    public DateTime StartTime { get; }
    public TimeSpan Timeout { get; }
    public TaskCompletionSource<Device> DeviceCandidate { get; }
    public bool IsOccupied { get; set; } = false;
    public bool IsClosed { get; private set; }
    public bool IsDeviceApproved { get; set; }
    private TaskCompletionSource DeviceApproval { get; }

    public RegistrationSession(string token, DateTime startTime, TimeSpan timeout)
    {
        Token = token;
        StartTime = startTime;
        Timeout = timeout;
        DeviceCandidate = new TaskCompletionSource<Device>();
        DeviceApproval = new TaskCompletionSource();
    }

    public DateTime GetExpireDate() => StartTime + Timeout;
    public bool IsExpired() => DateTime.Now >= GetExpireDate();
    public TimeSpan GetTimeLeft() => GetExpireDate() - DateTime.Now;

    public void Close()
    {
        DeviceCandidate.SetCanceled();
        if(IsDeviceApproved)
            DeviceApproval.SetResult();
        else
            DeviceApproval.SetCanceled();
            
        IsClosed = true;
    }

    public Task GetApprovalHandle() => DeviceApproval.Task;
}