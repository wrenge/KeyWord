using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using KeyWord.Communication;

namespace KeyWord.Client.Services
{
    public class RegisterService : INetworkService
    {
        private readonly HttpClient _client;

        public RegisterService(HttpClient client)
        {
            _client = client;
        }

        public RegisterService(Uri baseAddress)
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        
        public async Task<bool> TryRegister(string id, string name, IPAddress host, string token)
        {
            var deviceInfo = new DeviceCandidate()
            {
                Id = id,
                Name = name,
                Token = token
            };
            var json = JsonSerializer.Serialize(deviceInfo);
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var postResponse = await _client.PostAsync("/Register/PostDeviceInfo", content);
            if (!postResponse.IsSuccessStatusCode)
                return false; // TODO детализировать ошибку

            var idUrlEncoded = HttpUtility.UrlEncode(deviceInfo.Id);
            var approvalResponse = await _client.GetAsync($"Register/GetDeviceApproval/{idUrlEncoded}");
            return approvalResponse.IsSuccessStatusCode;
        }
    }
}