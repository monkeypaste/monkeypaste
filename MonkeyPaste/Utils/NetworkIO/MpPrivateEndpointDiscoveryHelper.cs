using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MonkeyPaste {
    public class MpPrivateEndpointDiscoveryHelper {
        public static async Task<TcpClient> Discover(string thisIp, int port, bool isWpf) {
            var tcpc = await FindPrivateListenerIp4(thisIp, port, isWpf);
            return tcpc;
        }

        private static async Task<TcpClient> FindPrivateListenerIp4(string thisIp, int port, bool isWpf) {
            // TODO need to test in different network scenerios may need use
            // private helper string[] GetAllIp4 and scan all local up addresses
            // on LAN's with actual routers...
            var ipParts = thisIp.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            string ipPrefix = string.Format("{0}.{1}.{2}.", ipParts[0], ipParts[1], ipParts[2]);
            var taskList = new List<Task<string>>();
            for (int i = 1; i < 255; i++) {
                string b4 = i.ToString();
                if (b4 == ipParts[3]) {
                    //skip this ip
                    continue;
                }
                string ip = ipPrefix + b4;
                var tcpc = TestConnection(ip, port, isWpf);
                if (tcpc != null) {
                    return tcpc;
                } else {
                    await Task.Delay(0);
                }
            }
            return null;
        }

        private static TcpClient TestConnection(string ip, int port, bool isWpf) {
            if (isWpf) {
                return TestConnection_Wpf(ip, port);
            }
            return TestConnection_Mobile_Tcp(ip, port);
        }

        private static TcpClient TestConnection_Wpf(String hostname, int portno) {
            //bool connected = true;
            var tcp = new TcpClient();
            IAsyncResult ar = tcp.BeginConnect(hostname, portno, null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100), false)) {
                    //Console.WriteLine("Tried " + hostname);
                    tcp.Close();
                    tcp.Dispose();
                    tcp = null;
                    //connected = false;
                } else {
                    tcp.EndConnect(ar);
                }
            } finally {
                wh.Close();
            }
            return tcp;
        }

        private static TcpClient TestConnection_Mobile_Tcp(string hostname, int portno) {
            //string test = @"192.168.43.209";
            //hostname = test;
            var tcpc = new TcpClient();
            tcpc.SendTimeout = 25;
            tcpc.ReceiveTimeout = 25;
            try {
                tcpc.Connect(hostname, portno);
                //if (tcpc.Connected) {
                //    return tcpc;
                //}
                //tcpc.Close();
                //tcpc.Dispose();
                //tcpc = null;
            }
            catch (Exception) {
                if(tcpc != null && !tcpc.Connected) {
                    tcpc.Close();
                    tcpc.Dispose();
                    tcpc = null;
                }
                return tcpc;
            }
            return tcpc;
        }

        private static bool TestConnection_Mobile_Socket(string hostname, int portno) {
            IPAddress ipa = (IPAddress)Dns.GetHostAddresses(hostname)[0];
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                try {
                    sock.SendTimeout = 25;
                    sock.Connect(ipa, portno);
                    if (sock.Connected) {
                        sock.Close();
                        return true;
                    }
                    sock.Close();

                }
                catch (SocketException) {
                    if (sock != null) {
                        sock.Close();
                    }
                    return false;
                }
            }
            return false;
        }
    }
}
