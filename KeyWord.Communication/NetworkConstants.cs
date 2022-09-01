using System.Text;

namespace KeyWord.Communication
{
    public static class NetworkConstants
    {
        private const string DiscoveryRequestPrefix = "KW_REQ";
        private const string DiscoveryResponsePrefix = "KW_RESP";
        public static string DiscoveryRequestPattern => $"{DiscoveryRequestPrefix} {{0}}";
        public static string DiscoveryRequestRegex => $@"{DiscoveryRequestPrefix} (.*)";
        public const string DiscoveryRequestSalt = "RequestSalt";
        public static string DiscoveryResponsePattern => $"{DiscoveryResponsePrefix} {{0}}";
        public static string DiscoveryResponseRegex => $@"{DiscoveryResponsePrefix} (.*)";
        public const string DiscoveryResponseSalt = "ResponseSalt";
        public const int DiscoveryKeyLength = 8;
        public const int DiscoveryIterations = 1;
        public static readonly Encoding DiscoveryEncoding = Encoding.ASCII;
        public const int Port = 8989;
    }
}