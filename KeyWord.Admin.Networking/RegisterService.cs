using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KeyWord.Communication;

namespace KeyWord.Admin.Networking
{
    public class RegisterService : INetworkService
    {
        public async Task StartNewRegistration()
        {
            var uriBuilder = new UriBuilder("http", "localhost");
            uriBuilder.Path = "Register/StartNewRegistration"; // TODO вынести в константы
            var client = new HttpClient();
            
            var response = await client.GetAsync(uriBuilder.Uri);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());;
        }
        
        public async Task<RegisterInfo> RequestNewToken()
        {
            var uriBuilder = new UriBuilder("http", "localhost");
            uriBuilder.Path = "Register/RequestNewToken"; // TODO вынести в константы
            var client = new HttpClient();
            
            var response = await client.GetAsync(uriBuilder.Uri);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var contentString = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<RegisterInfo>(contentString);
            return info!;
        }
        
        public async Task<DeviceCandidate> RequestDeviceCandidate()
        {
            var uriBuilder = new UriBuilder("http", "localhost");
            uriBuilder.Path = "Register/RequestDeviceCandidate"; // TODO вынести в константы
            var client = new HttpClient();
            
            var response = await client.GetAsync(uriBuilder.Uri);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var contentString = await response.Content.ReadAsStringAsync();
            var device = JsonSerializer.Deserialize<DeviceCandidate>(contentString);
            return device!;
        }
        
        public async Task ApprovePendingDevice()
        {
            var uriBuilder = new UriBuilder("http", "localhost");
            uriBuilder.Path = "Register/ApprovePendingDevice"; // TODO вынести в константы
            var client = new HttpClient();
            
            var postResponse = await client.PostAsync(uriBuilder.Uri, null);
            if (!postResponse.IsSuccessStatusCode)
                throw new Exception(await postResponse.Content.ReadAsStringAsync());
        }
        
        public async Task DenyPendingDevice()
        {
            var uriBuilder = new UriBuilder("http", "localhost");
            uriBuilder.Path = "Register/DenyPendingDevice"; // TODO вынести в константы
            var client = new HttpClient();
            
            var postResponse = await client.PostAsync(uriBuilder.Uri, null);
            if (!postResponse.IsSuccessStatusCode)
                throw new Exception(await postResponse.Content.ReadAsStringAsync());
        }
    }
}