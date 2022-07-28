using System;
using System.Security.Cryptography;

namespace KeyWord.Crypto
{
    public class Pbkdf2 : IKeyDerivationAlgorithm
    {
        public int Iterations { get; set; }
        public int Length { get; set; }

        public Pbkdf2(int iterations, int length)
        {
            Iterations = iterations > 0 ? iterations : throw new ArgumentException(nameof(iterations));
            Length = length > 0 ? length : throw new ArgumentException(nameof(length));
        }

        public ByteText ComputeKey(ByteText password, ByteText salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password.Bytes, salt.Bytes, Iterations);
            return new ByteText(pbkdf2.GetBytes(Length));
        }
    }
}