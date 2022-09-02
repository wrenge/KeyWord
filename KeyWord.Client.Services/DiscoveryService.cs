using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KeyWord.Communication;

namespace KeyWord.Client.Services
{
    public class DiscoveryService
    {
        public async Task<IPAddress?> DiscoverServer(int port, string token, TimeSpan timeout)
        {
            using var client = new UdpClient(NetworkConstants.ResponsePort);
            client.EnableBroadcast = true;
            
            var requestCode = SyncUtilities.GetDiscoveryRequestAuthKey(token);
            var expectedResponseCode = SyncUtilities.GetDiscoveryResponseAuthKey(token).ToBase64();
            var requestString = string.Format(NetworkConstants.DiscoveryRequestPattern, requestCode.ToBase64());
            var requestData = NetworkConstants.DiscoveryEncoding.GetBytes(requestString);

            await client.SendAsync(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, port));

            IPAddress? result = null;

            async Task ReceiveData()
            {
                var responseCode = "";
                UdpReceiveResult responseResult;
                while (expectedResponseCode != responseCode)
                {
                    responseResult = await client.ReceiveAsync();
                    var responseData = responseResult.Buffer;
                    var responseString = NetworkConstants.DiscoveryEncoding.GetString(responseData);
                    try
                    {
                        var match = Regex.Match(responseString, NetworkConstants.DiscoveryResponseRegex);
                        responseCode = match.Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                result = responseResult.RemoteEndPoint.Address;
            }

            async Task Wait() => await Task.Delay(timeout);

            await Task.WhenAny(ReceiveData(), Wait());

            return result;
        }
    }
}