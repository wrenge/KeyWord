using System;
using System.Security.Cryptography;

namespace Wrenge.KeyWord.Crypto
{
    public class Pbkdf2 : IKeyDerivationAlgorithm
    {
        public byte[]? Password { get; set; }
        public byte[]? Salt { get; set; }
        public int Iterations { get; set; }
        public int Length { get; set; }

        public Pbkdf2(int iterations, int length)
        {
            Iterations = iterations > 0 ? iterations : throw new ArgumentException(nameof(iterations));
            Length = length > 0 ? length : throw new ArgumentException(nameof(length));
        }

        public byte[] ComputeKey()
        {
            if (Password == null)
                throw new ArgumentNullException(nameof(Password));
            if (Salt == null)
                throw new ArgumentNullException(nameof(Salt));

            using var pbkdf2 = new Rfc2898DeriveBytes(Password, Salt, Iterations);
            return pbkdf2.GetBytes(Length);
        }
    }
}