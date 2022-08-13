using System;
using System.Net;

namespace KeyWord.Communication
{
    public class RegisterInfo
    {
        public IPAddress ServerAddress;
        public string Token;
        public DateTime ExpireDate;

        public RegisterInfo(string token, DateTime expireDate, IPAddress serverAddress)
        {
            ServerAddress = serverAddress;
            Token = token;
            ExpireDate = expireDate;
        }
    }
}