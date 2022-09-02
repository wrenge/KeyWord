using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using KeyWord.Communication;

namespace KeyWord.Admin.Services
{
    public class DevicesService
    {
        private readonly HttpClient _client;

        public DevicesService(HttpClient client)
        {
            _client = client;
        }
        
        public async Task<IEnumerable<Device>> GetDevicesList()
        {
            var response = await _client.GetAsync("Devices/GetDeviceList");
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Device[]>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return result!;
        }

        public async Task<bool> RemoveDevice(string id)
        {
            var idUrlEncoded = HttpUtility.UrlEncode(id);
            var response = await _client.DeleteAsync($"Devices/DeleteDevice/{idUrlEncoded}");
            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;
            
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
            
            return true;
        }

        public async Task<bool> RenameDevice(string id, string name)
        {
            var json = JsonSerializer.Serialize(new RenameDeviceRequestData(id, name));
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PutAsync("Devices/RenameDevice", content);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;
            
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
            
            return true;
        }
    }
}