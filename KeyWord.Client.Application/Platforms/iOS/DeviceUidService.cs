using KeyWord.Client.Application.iOS;
using UIKit;

[assembly: Dependency(typeof(DeviceUidService))]
namespace KeyWord.Client.Application.iOS
{
    public class DeviceUidService : IDeviceUidService
    {
        public string GetUid()
        {
            var id = UIDevice.CurrentDevice.IdentifierForVendor.AsString();
            return id;
        }
    }
}