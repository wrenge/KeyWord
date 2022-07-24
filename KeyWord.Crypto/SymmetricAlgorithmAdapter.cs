using System;
using System.IO;
using System.Security.Cryptography;

namespace Wrenge.KeyWord.Crypto
{
    public abstract class SymmetricAlgorithmAdapter<T> : ICipherAlgorithm where T : SymmetricAlgorithm, new()
    {
        public byte[]? Value { get; set; }
        public byte[]? Key { get; set; }
        public byte[]? Vector { get; set; }
        protected CipherMode CipherMode;
        protected PaddingMode PaddingMode;

        public virtual byte[] Encrypt()
        {
            using var cipher = new T();
            cipher.Mode = CipherMode;
            cipher.Padding = PaddingMode;
            using var encryptor = cipher.CreateEncryptor(Key, Vector);
            using var to = new MemoryStream();
            using var writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write);
            writer.Write(Value, 0, Value.Length);
            writer.FlushFinalBlock();
            var encrypted = to.ToArray();

            return encrypted;
        }

        public virtual byte[] Decrypt()
        {
            using var cipher = new T();
            cipher.Mode = CipherMode;
            cipher.Padding = PaddingMode;
            using var decryptor = cipher.CreateDecryptor(Key, Vector);
            using var to = new MemoryStream();
            using var writer = new CryptoStream(to, decryptor, CryptoStreamMode.Write);
            writer.Write(Value, 0, Value.Length);
            writer.FlushFinalBlock();
            var decrypted = to.ToArray();

            return decrypted;
        }
    }
}