using KeyWord.Client.Application.iOS;
using KeyWord.Client.Application.Services;
using UIKit;
using Xamarin.Forms;

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