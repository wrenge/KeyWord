namespace KeyWord.Crypto
{
    public interface ICipherAlgorithm
    {
        byte[] Vector { get; set; }
        ByteText Encrypt(ByteText text, ByteText key);
        ByteText Decrypt(ByteText text, ByteText key);
    }
}