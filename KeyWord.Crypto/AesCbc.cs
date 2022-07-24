using System.Security.Cryptography;

namespace Wrenge.KeyWord.Crypto
{
    public sealed class AesCbc : SymmetricAlgorithmAdapter<AesManaged>
    {
        public AesCbc()
        {
            CipherMode = CipherMode.CBC;
            PaddingMode = PaddingMode.ISO10126;
        }
    }
}