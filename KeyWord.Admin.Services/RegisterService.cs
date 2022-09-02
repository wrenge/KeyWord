using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KeyWord.Communication;

namespace KeyWord.Admin.Services
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
        
        public async Task StartNewRegistration()
        {
            var response = await _client.PostAsync("Register/StartNewRegistration", null);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
        
        public async Task<RegisterInfo> RequestNewToken()
        {
            var response = await _client.GetAsync("Register/RequestNewToken");
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var contentString = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<RegisterInfo>(contentString, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return info!;
        }
        
        public async Task<DeviceCandidate> RequestDeviceCandidate()
        {
            var response = await _client.GetAsync("Register/RequestDeviceCandidate");
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var contentString = await response.Content.ReadAsStringAsync();
            var device = JsonSerializer.Deserialize<DeviceCandidate>(contentString, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return device!;
        }
        
        public async Task ApprovePendingDevice()
        {
            var postResponse = await _client.PostAsync("Register/ApprovePendingDevice", null);
            if (!postResponse.IsSuccessStatusCode)
                throw new Exception(await postResponse.Content.ReadAsStringAsync());
        }
        
        public async Task DenyPendingDevice()
        {
            var postResponse = await _client.PostAsync("Register/DenyPendingDevice", null);
            if (!postResponse.IsSuccessStatusCode)
                throw new Exception(await postResponse.Content.ReadAsStringAsync());
        }
    }
}