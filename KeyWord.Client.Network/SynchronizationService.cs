using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KeyWord.Communication;
using KeyWord.Credentials;

namespace KeyWord.Client.Network
{
    public class SynchronizationService : INetworkService
    {
        public string HostName { get; set; } = "";
        
        public async Task<SyncData?> TrySync(string deviceId,
            string deviceToken,
            DateTime lastSyncTime,
            IEnumerable<ClassicCredentialsInfo> added,
            IEnumerable<ClassicCredentialsInfo> modified,
            IEnumerable<int> deleted)
        {
            var uriBuilder = new UriBuilder("http", HostName);
            uriBuilder.Path = "Sync/RequestSync"; // TODO вынести в константы
            var client = new HttpClient();
            var syncRequest = new SyncRequest
            {
                DeviceId = deviceId,
                AuthKey = SyncUtilities.GetDeviceAuthKey(deviceId, deviceToken).ToBase64(),
                LastSyncTime = lastSyncTime,
                SyncData = new SyncData
                {
                    AddedCredentials = added.ToArray(),
                    ModifiedCredentials = modified.ToArray(),
                    DeletedCredentialsIds = deleted.ToArray()
                }
            };
            
            var json = JsonSerializer.Serialize(syncRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var postResponse = await client.PostAsync(uriBuilder.Uri, content);
            if (!postResponse.IsSuccessStatusCode)
                return null; // TODO детализировать ошибку

            var responseContent = await postResponse.Content.ReadAsStringAsync();
            var syncResponse = JsonSerializer.Deserialize<SyncResponse>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return syncResponse?.SyncData;
        }
    }
}