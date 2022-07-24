using System;
using System.Security.Cryptography;
using System.Text;

namespace Wrenge.KeyWord.Crypto
{
    public sealed class Cipher
    {
        private ICipherAlgorithm _cipher;
        private IKeyDerivationAlgorithm _kd;

        public Cipher(ICipherAlgorithm cipher, IKeyDerivationAlgorithm kd)
        {
            _cipher = cipher ?? throw new ArgumentNullException(nameof(cipher));
            _kd = kd ?? throw new ArgumentNullException(nameof(kd));
        }

        public string Encrypt(string value, string password, string salt, string iv)
        {
            if (_cipher == null)
                throw new ArgumentNullException(nameof(_cipher));
            if (_kd == null)
                throw new ArgumentNullException(nameof(_kd));

            var valueBytes = Encoding.Default.GetBytes(value);
            var saltBytes = Encoding.Default.GetBytes(salt);
            var passwordBytes = Encoding.Default.GetBytes(password);
            var ivBytes = Encoding.Default.GetBytes(iv);

            _kd.Password = passwordBytes;
            _kd.Salt = saltBytes;

            _cipher.Value = valueBytes;
            _cipher.Vector = ivBytes;
            _cipher.Key = _kd.ComputeKey();
            var cipherBytes = _cipher.Encrypt();

            return Convert.ToBase64String(cipherBytes);
        }
        
        public string Decrypt(string encrypted, string password, string salt, string iv)
        {
            if (_cipher == null)
                throw new ArgumentNullException(nameof(_cipher));
            if (_kd == null)
                throw new ArgumentNullException(nameof(_kd));

            var cipherBytes = Convert.FromBase64String(encrypted);
            var saltBytes = Encoding.Default.GetBytes(salt);
            var passwordBytes = Encoding.Default.GetBytes(password);
            var ivBytes = Encoding.Default.GetBytes(iv);

            _kd.Password = passwordBytes;
            _kd.Salt = saltBytes;

            _cipher.Value = cipherBytes;
            _cipher.Vector = ivBytes;
            _cipher.Key = _kd.ComputeKey();

            var valueBytes = _cipher.Decrypt();

            return Encoding.Default.GetString(valueBytes);
        }
    }
}