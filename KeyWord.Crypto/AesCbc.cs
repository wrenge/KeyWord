using System.Security.Cryptography;

namespace KeyWord.Crypto
{
    public sealed class AesCbc : SymmetricAlgorithmAdapter<AesManaged>
    {
        public AesCbc(ByteText vector) : base(vector)
        {
            CipherMode = CipherMode.CBC;
            PaddingMode = PaddingMode.ISO10126;
        }
    }
}