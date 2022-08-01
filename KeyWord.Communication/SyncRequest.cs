namespace KeyWord.Communication
{
    [System.Serializable]
    public class SyncRequest
    {
        public string AuthToken { get; set; }
        public string DeviceId { get; set; }
        public SyncData SyncData { get; set; } = new SyncData(); // TODO: encrypt that
    }
}