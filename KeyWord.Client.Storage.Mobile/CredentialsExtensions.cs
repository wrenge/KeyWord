using KeyWord.Credentials;
using KeyWord.Crypto;
using System;

namespace KeyWord.Client.Storage.Mobile
{
    public static class CredentialsExtensions
    {
        public static ClassicCredentialsInfo GetClassicEncrypted(this ICredentialsInfo info, ByteText key)
        {
            var result = new ClassicCredentialsInfo(info);
            EncryptClassic(result, key);
            return result;
        }

        public static ClassicCredentialsInfo EncryptClassic(this ClassicCredentialsInfo info, ByteText key)
        {
            var cipher = new AesCbc(new ByteText(CryptoConstants.InitialVector));
            info.Login = cipher.Encrypt(new ByteText(info.Login), key).ToBase64();
            info.Password = cipher.Encrypt(new ByteText(info.Password), key).ToBase64();
            return info;
        }

        public static ClassicCredentialsInfo GetClassicDecrypted(this ICredentialsInfo info, ByteText key)
        {
            var result = new ClassicCredentialsInfo(info);
            DecryptClassic(result, key);
            return result;
        }

        public static ClassicCredentialsInfo DecryptClassic(this ClassicCredentialsInfo info, ByteText key)
        {
            var cipher = new AesCbc(new ByteText(CryptoConstants.InitialVector));
            info.Login = cipher.Decrypt(new ByteText(Convert.FromBase64String(info.Login)), key).ToString();
            info.Password = cipher.Decrypt(new ByteText(Convert.FromBase64String(info.Password)), key).ToString();
            return info;
        }
    }
}