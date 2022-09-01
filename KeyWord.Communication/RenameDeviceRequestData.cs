namespace KeyWord.Communication
{
    public class RenameDeviceRequestData
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public RenameDeviceRequestData(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}