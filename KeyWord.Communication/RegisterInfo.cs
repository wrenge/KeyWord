using System;
using System.Net;

namespace KeyWord.Communication
{
    public class RegisterInfo
    {
        public string ServerAddress { get; }
        public string Token { get; }
        public DateTime ExpireDate { get; }

        public RegisterInfo(string token, DateTime expireDate, IPAddress serverAddress)
        {
            ServerAddress = serverAddress.ToString();
            Token = token;
            ExpireDate = expireDate;
        }
    }
}