using System;
using System.Text;

namespace KeyWord.Crypto
{
    public struct ByteText
    {
        public byte[] Bytes { get; }

        public ByteText(byte[] bytes)
        {
            Bytes = bytes;
        }

        public ByteText(string text)
        {
            Bytes = Encoding.Default.GetBytes(text);
        }

        public override string ToString() => Encoding.Default.GetString(Bytes);
        public string ToBase64() => Convert.ToBase64String(Bytes);
    }
}