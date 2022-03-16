using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Xamarin.Essentials;
using System.Linq;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public static class MpNetworkHelpers {
        public static string GetIpForDomain(string domain) {
            if (string.IsNullOrEmpty(domain)) {
                return "0.0.0.0";
            }
            var al = Dns.GetHostAddresses(domain).ToList();
            foreach (var a in al) {
                if (a.AddressFamily == AddressFamily.InterNetwork) {
                    return a.ToString();
                }
            }
            return "0.0.0.0";
        }

        public static bool IsConnectedToInternet() {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet) {
                return true;
            }
            return false;
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
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up && !item.Description.ToLower().Contains("virtual")) {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }

        public static string GetExternalIp4Address() {
            return new System.Net.WebClient().DownloadString("https://api.ipify.org");
        }
    }
}
