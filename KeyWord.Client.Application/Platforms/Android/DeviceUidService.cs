using Android.Provider;
using KeyWord.Client.Application.Android;
using KeyWord.Client.Application.Services;

[assembly: Dependency(typeof(DeviceUidService))]
namespace KeyWord.Client.Application.Android
{
    public class DeviceUidService : IDeviceUidService
    {
        public string GetUid()
        {
            var context = MainApplication.Context;
            var id = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
            return id;
        }
    }
}