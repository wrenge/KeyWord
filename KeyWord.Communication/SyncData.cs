using System;
using KeyWord.Credentials;

namespace KeyWord.Communication
{
    public class SyncData
    {
        public ClassicCredentialsInfo[] AddedCredentials { get; set; } = Array.Empty<ClassicCredentialsInfo>();
        public ClassicCredentialsInfo[] ModifiedCredentials { get; set; } = Array.Empty<ClassicCredentialsInfo>();
        public int[] DeletedCredentialsIds { get; set; } = Array.Empty<int>();
    }
}