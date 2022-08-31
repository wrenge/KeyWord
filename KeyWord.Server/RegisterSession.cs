using KeyWord.Communication;

namespace KeyWord.Server;

public class RegisterSession
{
    public string Token { get; }
    public DateTime StartTime { get; }
    public TimeSpan Timeout { get; }
    public TaskCompletionSource<DeviceCandidate> DeviceCandidate { get; }
    public bool IsExpired => DateTime.Now >= GetExpireDate();
    public bool IsOccupied { get; set; } = false;
    public bool IsClosed { get; private set; }
    public bool IsDeviceApproved { get; set; }
    private TaskCompletionSource DeviceApproval { get; }

    public RegisterSession(string token, DateTime startTime, TimeSpan timeout)
    {
        Token = token;
        StartTime = startTime;
        Timeout = timeout;
        DeviceCandidate = new TaskCompletionSource<DeviceCandidate>();
        DeviceApproval = new TaskCompletionSource();
    }

    public DateTime GetExpireDate() => StartTime + Timeout;
    
    public TimeSpan GetTimeLeft() => GetExpireDate() - DateTime.Now;

    public void Close()
    {
        DeviceCandidate.TrySetCanceled();
        if(IsDeviceApproved)
            DeviceApproval.SetResult();
        else
            DeviceApproval.SetCanceled();
            
        IsClosed = true;
    }

    public Task GetApprovalHandle() => DeviceApproval.Task;
}