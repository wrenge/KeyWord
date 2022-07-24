namespace Wrenge.KeyWord.Crypto
{
    public interface IKeyDerivationAlgorithm
    {
        byte[]? Password { get; set; }
        byte[]? Salt { get; set; }
        int Iterations { get; set; }
        int Length { get; set; }

        byte[] ComputeKey();
    }
}