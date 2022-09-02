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
        private readonly HttpClient _client;

        public SynchronizationService(HttpClient client)
        {
            _client = client;
        }

        public SynchronizationService(Uri baseAddress)
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        
        public async Task<SyncResponse?> TrySync(string deviceId,
            string deviceToken,
            string authId,
            DateTime lastSyncTime,
            IEnumerable<ClassicCredentialsInfo> added,
            IEnumerable<ClassicCredentialsInfo> modified,
            IEnumerable<int> deleted)
        {
            var authKey = SyncUtilities.GetDeviceAuthKey(deviceId, deviceToken).ToBase64();
            var syncRequest = new SyncRequest
            {
                DeviceId = deviceId,
                AuthKey = authKey,
                AuthId = authId,
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
            var postResponse = await _client.PostAsync("Sync/RequestSync", content);
            if (!postResponse.IsSuccessStatusCode)
                return null; // TODO детализировать ошибку

            var responseContent = await postResponse.Content.ReadAsStringAsync();
            var syncResponse = JsonSerializer.Deserialize<SyncResponse>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return syncResponse;
        }
    }
}