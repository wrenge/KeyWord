using System;
using System.Linq;
using System.Text;
using KeyWord.Crypto;

namespace KeyWord.Communication
{
    public static class SyncUtilities
    {
        public static ByteText GetDeviceAuthKey(string deviceId, string token)
        {
            var time = DateTime.UtcNow;
            time = new DateTime() + new TimeSpan(time.Hour, time.Minute, 0);
            var hasher = new Pbkdf2(CryptoConstants.KdIterations, CryptoConstants.KdLength);
            var bytes = Encoding.Default.GetBytes(deviceId)
                .Concat(Encoding.Default.GetBytes(token))
                .Concat(BitConverter.GetBytes(time.ToBinary()))
                .ToArray();
        
            return hasher.ComputeKey(new ByteText(bytes), new ByteText(CryptoConstants.KdSalt));
        }
    }
}