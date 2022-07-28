using System;
using System.IO;
using System.Security.Cryptography;

namespace KeyWord.Crypto
{
    public abstract class SymmetricAlgorithmAdapter<T> : ICipherAlgorithm where T : SymmetricAlgorithm, new()
    {
        protected SymmetricAlgorithmAdapter(ByteText vector)
        {
            Vector = vector.Bytes;
        }

        public byte[] Vector { get; set; }
        protected CipherMode CipherMode;
        protected PaddingMode PaddingMode;

        public virtual ByteText Encrypt(ByteText text, ByteText key)
        {
            if (Vector == null)
                throw new ArgumentNullException(null, nameof(Vector));
            
            using var cipher = new T();
            cipher.Mode = CipherMode;
            cipher.Padding = PaddingMode;
            using var encryptor = cipher.CreateEncryptor(key.Bytes, Vector);
            using var to = new MemoryStream();
            using var writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write);
            writer.Write(text.Bytes, 0, text.Bytes.Length);
            writer.FlushFinalBlock();
            var encrypted = to.ToArray();

            return new ByteText(encrypted);
        }

        public virtual ByteText Decrypt(ByteText text, ByteText key)
        {
            if (Vector == null)
                throw new ArgumentNullException(null, nameof(Vector));
            
            using var cipher = new T();
            cipher.Mode = CipherMode;
            cipher.Padding = PaddingMode;
            using var decryptor = cipher.CreateDecryptor(key.Bytes, Vector);
            using var to = new MemoryStream();
            using var writer = new CryptoStream(to, decryptor, CryptoStreamMode.Write);
            writer.Write(text.Bytes, 0, text.Bytes.Length);
            writer.FlushFinalBlock();
            var decrypted = to.ToArray();

            return new ByteText(decrypted);
        }
    }
}