using Android.Provider;
using KeyWord.Client.Application.Droid;
using KeyWord.Client.Application.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(DeviceUidService))]
namespace KeyWord.Client.Application.Droid
{
    public class DeviceUidService : IDeviceUidService
    {
        public string GetUid()
        {
            var context = global::Android.App.Application.Context;
            var id = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
            return id;
        }
    }
}