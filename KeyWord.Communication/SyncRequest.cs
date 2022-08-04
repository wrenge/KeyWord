using System;

namespace KeyWord.Communication
{
    [System.Serializable]
    public class SyncRequest
    {
        public string AuthKey { get; set; }
        public string DeviceId { get; set; }
        public DateTime LastSyncTime { get; set; }
        public SyncData SyncData { get; set; } = new SyncData(); // TODO: encrypt that
    }
}