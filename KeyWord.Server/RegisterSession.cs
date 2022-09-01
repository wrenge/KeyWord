using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using KeyWord.Communication;

namespace KeyWord.Server;

public class RegisterSession
{
    public string Token { get; }
    public int Port { get; }
    public DateTime StartTime { get; }
    public TimeSpan Timeout { get; }
    public TaskCompletionSource<DeviceCandidate> DeviceCandidate { get; }
    public bool IsExpired => DateTime.Now >= GetExpireDate();
    public bool IsOccupied { get; set; } = false;
    public bool IsClosed { get; private set; }
    public bool IsDeviceApproved { get; set; }
    private TaskCompletionSource DeviceApproval { get; }
    private UdpClient _udpServer;

    public RegisterSession(string token, DateTime startTime, TimeSpan timeout, int port)
    {
        Token = token;
        StartTime = startTime;
        Timeout = timeout;
        Port = port;
        DeviceCandidate = new TaskCompletionSource<DeviceCandidate>();
        DeviceApproval = new TaskCompletionSource();
        _udpServer = new UdpClient(port);
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
        
        _udpServer.Close();
        IsClosed = true;
    }

    public Task GetApprovalHandle() => DeviceApproval.Task;

    public async Task ListenDiscoveryAsync()
    {
        var responseCode = SyncUtilities.GetDiscoveryResponseAuthKey(Token);
        var expectedRequestCode = SyncUtilities.GetDiscoveryRequestAuthKey(Token).ToBase64();
        var responseString = string.Format(NetworkConstants.DiscoveryResponsePattern, responseCode.ToBase64());
        var responseData = NetworkConstants.DiscoveryEncoding.GetBytes(responseString);

        while (!IsClosed && !IsExpired)
        {
            var incoming = await _udpServer.ReceiveAsync();
            var receivedData = incoming.Buffer;
            var receivedString = Encoding.ASCII.GetString(receivedData);
            var requestCode = "";
            try
            {
                var match = Regex.Match(receivedString, NetworkConstants.DiscoveryRequestRegex);
                requestCode = match.Groups[1].Value;
            }
            catch (Exception)
            {
                // ignored
            }

            if (expectedRequestCode == requestCode)
                await _udpServer.SendAsync(responseData, responseData.Length, incoming.RemoteEndPoint);
        }
    }
}