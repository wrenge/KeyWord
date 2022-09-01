using System;
using System.Net;

namespace KeyWord.Communication
{
    public class RegisterInfo
    {
        public int ServerPort { get; }
        public string Token { get; }
        public DateTime ExpireDate { get; }

        public RegisterInfo(string token, DateTime expireDate, int port)
        {
            ServerPort = port;
            Token = token;
            ExpireDate = expireDate;
        }
    }
}