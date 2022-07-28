namespace KeyWord.Crypto
{
    public interface IKeyDerivationAlgorithm
    {
        int Iterations { get; set; }
        int Length { get; set; }

        ByteText ComputeKey(ByteText password, ByteText salt);
    }
}