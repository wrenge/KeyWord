using System;
using System.Net;

namespace KeyWord.Communication
{
    public class RegisterInfo
    {
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
        public int ServerPort { get; set; }

        public RegisterInfo(string token, DateTime expireDate, int serverPort)
        {
            ServerPort = serverPort;
            Token = token;
            ExpireDate = expireDate;
        }
    }
}