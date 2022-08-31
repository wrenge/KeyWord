using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    }
}