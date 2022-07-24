﻿namespace Wrenge.KeyWord.Crypto
{
    public interface IHashAlgorithm
    {
        byte[] Value { get; set; }
        byte[] ComputeHash();
    }
}