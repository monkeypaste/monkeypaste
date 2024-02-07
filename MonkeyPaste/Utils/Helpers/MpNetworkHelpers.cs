using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
//using Xamarin.Forms;
using System.IO;
//using Xamarin.Essentials;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpNetworkHelpers {
        public const string UNKNOWN_DOMAIN = "0.0.0.0";
        public const string THIS_APP_URL = @"https://www.monkeypaste.com";
        public static string GetIpForDomain(string domain) {
            if (string.IsNullOrEmpty(domain)) {
                return UNKNOWN_DOMAIN;
            }
            var al = Dns.GetHostAddresses(domain).ToList();
            foreach (var a in al) {
                if (a.AddressFamily == AddressFamily.InterNetwork) {
                    return a.ToString();
                }
            }
            return UNKNOWN_DOMAIN;
        }

        public static bool IsConnectedToInternet() {

            //var current = Connectivity.NetworkAccess;

            //if (current == NetworkAccess.Internet) {
            //    return true;
            //}
            //return false;]
            return GetIpForDomain(THIS_APP_URL) != UNKNOWN_DOMAIN;
        }
        public static bool IsConnectedToNetwork() {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

        }

        public static bool IsMpServerAvailable() {
            if (!IsConnectedToNetwork()) {
                return false;
            }
            try {

                using (var client = new WebClient()) {
                    try {
                        var stream = client.OpenRead(@"https://www.monkeypaste.com/");
                        stream.Dispose();
                        return true;
                    }
                    catch (System.AggregateException ex) {
                        MpConsole.WriteTraceLine("Sync Server Unavailable", ex);
                        return false;
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine("Sync Server Unavailable", ex);
                        return false;
                    }
                }
            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
                return false;
            }
        }

        public static string GetLocalIp4Address() {
            var ips = GetAllLocalIPv4(NetworkInterfaceType.Wireless80211);
            if (ips.Length > 0) {
                return ips[0];
            }
            ips = GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
            if (ips.Length > 0) {
                return ips[0];
            }
            return "0.0.0.0";
        }

        public static string[] GetAllLocalIPv4() {
            var ips = GetAllLocalIPv4(NetworkInterfaceType.Wireless80211).ToList();
            ips.AddRange(GetAllLocalIPv4(NetworkInterfaceType.Ethernet));
            return ips.ToArray();
        }

        private static string[] GetAllLocalIPv4(NetworkInterfaceType _type) {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {
                if (item.NetworkInterfaceType == _type &&
                    item.OperationalStatus == OperationalStatus.Up &&
                    !item.Description.ToLowerInvariant().Contains("virtual")) {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }

        public static async Task<string> GetExternalIp4AddressAsync() {
            //string result = await MpUrlHelpers.ReadUrlAsString("https://api.ipify.org");
            //return result;

            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

            request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse()) {
                using (var reader = new StreamReader(response.GetResponseStream())) {
                    publicIPAddress = await reader.ReadToEndAsync();
                }
            }

            return publicIPAddress.Replace("\n", "");
        }
    }
}
