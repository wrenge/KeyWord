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
        
        public static ByteText GetDiscoveryRequestAuthKey(string token)
        {
            var requestSalt = NetworkConstants.DiscoveryRequestSalt;
            return GetDiscoveryAuthKey(token, requestSalt);
        }
        
        public static ByteText GetDiscoveryResponseAuthKey(string token)
        {
            var requestSalt = NetworkConstants.DiscoveryResponseSalt;
            return GetDiscoveryAuthKey(token, requestSalt);
        }

        private static ByteText GetDiscoveryAuthKey(string token, string requestSalt)
        {
            var hasher = new Pbkdf2(NetworkConstants.DiscoveryIterations, NetworkConstants.DiscoveryKeyLength);
            var bytes = Encoding.Default.GetBytes(token).ToArray();

            return hasher.ComputeKey(new ByteText(bytes), new ByteText(requestSalt));
        }

        public static ByteText GetAuthId(string password)
        {
            var hasher = new Pbkdf2(NetworkConstants.AuthIdIterations, NetworkConstants.AuthIdKeyLength);
            var bytes = Encoding.Default.GetBytes(password).ToArray();

            return hasher.ComputeKey(new ByteText(bytes), new ByteText(NetworkConstants.AuthIdSalt));
        }
    }
}