using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace MonkeyPaste {
    public class MpPrivateEndpointDiscoveryHelper {
        public static async Task<Socket> Discover(string thisIp, int port, bool isWpf) {
            var sw = new Stopwatch();
            sw.Start();
            MpConsole.WriteLine(@"Attempting Listener discovery...");
            var socket = await FindPrivateListenerIp4(thisIp, port, isWpf);
            sw.Stop();
            MpConsole.WriteLine(@"Private ip sweep took {0} ms" + sw.ElapsedMilliseconds);
            if (socket != null) {
                MpConsole.WriteLine(@"Found listener");
                return socket;
            }
            MpConsole.WriteLine(@"No listener found, creating listener");
            return null;
        }

        private static async Task<Socket> FindPrivateListenerIp4(string thisIp, int port, bool isWpf) {
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
                var s = TestConnection(ip, port, isWpf);
                if (s != null) {
                    return s;
                } else {
                    await Task.Delay(0);
                }
            }
            return null;
        }

        private static Socket TestConnection(string ip, int port, bool isWpf) {
            if (isWpf) {
                return TestConnection_Wpf(ip, port);
            }
            return TestConnection_Mobile_Socket(ip, port);
        }

        private static Socket TestConnection_Wpf(String hostname, int portno) {
            var tcp = new TcpClient();
            IAsyncResult ar = tcp.BeginConnect(hostname, portno, null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100), false)) {
                    tcp.Close();
                    tcp.Dispose();
                    tcp = null;
                } else {
                    tcp.EndConnect(ar);
                }
            } finally {
                wh.Close();
            }
            if(tcp != null && tcp.Client.Connected) {
                return tcp.Client;
            }
            return null;
        }

        private static Socket TestConnection_Mobile_Tcp(string hostname, int portno) {
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
                return null;
            }
            if (tcpc != null && tcpc.Client.Connected) {
                return tcpc.Client;
            }
            return null;
        }

        private static Socket TestConnection_Mobile_Socket(string hostname, int portno) {
            //IPAddress ipa = (IPAddress)Dns.GetHostAddresses(hostname)[0];
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                try {
                    sock.SendTimeout = 25;
                    sock.Connect(IPAddress.Parse(hostname), portno);
                    if (sock.Connected) {                        
                        //sock.Close();
                        return sock;
                    }
                    sock.Close();

                }
                catch (SocketException) {
                    if (sock != null) {
                        sock.Close();
                    }
                    return null;
                }
            }
            return null;
        }
    }
}
