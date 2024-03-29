﻿using KeyWord.Client.Application.MacCatalyst;
using KeyWord.Client.Application.Services;
using UIKit;

[assembly: Dependency(typeof(DeviceUidService))]
namespace KeyWord.Client.Application.MacCatalyst
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