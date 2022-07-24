using System;

namespace Wrenge.KeyWord.Crypto
{
    public interface ICipherAlgorithm
    {
        byte[]? Value { get; set; }
        byte[]? Key { get; set; }
        byte[]? Vector { get; set; }
        byte[] Encrypt();
        byte[] Decrypt();
    }
}