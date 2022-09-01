using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KeyWord.Communication;

namespace KeyWord.Client.Network
{
    public class RegisterService : INetworkService
    {
        public string HostName { get; set; } = "";

        public async Task<bool> TryRegister(string id, string name, string token)
        {
            var uriBuilder = new UriBuilder("http", HostName);
            uriBuilder.Path = "Register/PostDeviceInfo"; // TODO вынести в константы
            var client = new HttpClient();
            var deviceInfo = new DeviceCandidate()
            {
                Id = id,
                Name = name,
                Token = token
            };
            var json = JsonSerializer.Serialize(deviceInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var postResponse = await client.PostAsync(uriBuilder.Uri, content);
            if (!postResponse.IsSuccessStatusCode)
                return false; // TODO детализировать ошибку

            uriBuilder.Path = "Register/GetDeviceApproval"; // TODO вынести в константы
            uriBuilder.Query = $"deviceId={deviceInfo.Id}";
            var approvalResponse = await client.GetAsync(uriBuilder.Uri);
            return approvalResponse.IsSuccessStatusCode;
        }

        public async Task<IPAddress?> DiscoverServer(int port, string token)
        {
            using var client = new UdpClient();
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            client.EnableBroadcast = true;
            
            var requestCode = SyncUtilities.GetDiscoveryRequestAuthKey(token);
            var expectedResponseCode = SyncUtilities.GetDiscoveryResponseAuthKey(token).ToBase64();
            var requestString = string.Format(NetworkConstants.DiscoveryRequestPattern, requestCode.ToBase64());
            var requestData = NetworkConstants.DiscoveryEncoding.GetBytes(requestString);

            await client.SendAsync(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, port));

            IPAddress? result = null;

            async Task ReceiveData()
            {
                var responseCode = "";
                UdpReceiveResult responseResult;
                while (expectedResponseCode != responseCode)
                {
                    responseResult = await client.ReceiveAsync();
                    var responseData = responseResult.Buffer;
                    var responseString = NetworkConstants.DiscoveryEncoding.GetString(responseData);
                    try
                    {
                        var match = Regex.Match(responseString, NetworkConstants.DiscoveryResponseRegex);
                        responseCode = match.Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                result = responseResult.RemoteEndPoint.Address;
            }

            async Task Wait() => await Task.Delay(TimeSpan.FromSeconds(30));

            await Task.WhenAny(ReceiveData(), Wait());

            return result;
        }
    }
}