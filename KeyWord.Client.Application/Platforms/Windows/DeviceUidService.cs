using Windows.System.Profile;
using KeyWord.Client.Application.Windows;

[assembly: Dependency(typeof(DeviceUidService))]
namespace KeyWord.Client.Application.Windows
{
    public class DeviceUidService : IDeviceUidService
    {
        public string GetUid()
        {
            var id = SystemIdentification.GetSystemIdForPublisher().Id.ToString();
            return id;
        }
    }
}