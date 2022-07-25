using System;

namespace KeyWord.Credentials
{
    public struct ClassicCredentialsInfo : ICredentialsInfo
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ModificationTime { get; set; }
    }
}