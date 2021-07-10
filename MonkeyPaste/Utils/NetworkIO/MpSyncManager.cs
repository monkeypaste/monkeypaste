using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpSyncManager : IDisposable {
        #region Singleton
        private static readonly Lazy<MpSyncManager> _Lazy = new Lazy<MpSyncManager>(() => new MpSyncManager());
        public static MpSyncManager Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Variables
        private MpSyncMesageType _lastMessageType = MpSyncMesageType.None;
        private MpISync _localSyncData;
        private bool _isListener;
        private string _lastMsg = string.Empty;

        private string _lastSend, _lastReceive;

        private TcpListener _listener;
        private TcpClient _client;
        #endregion

        public event EventHandler<string> OnError;
        #region Properties
        //public MpSocketClient SocketClient { get; set; } = new MpSocketClient();
        //public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        //public MpSslTcpClient SslClient { get; set; } = new MpSslTcpClient(null);
        //public MpSslTcpServer SslServer { get; set; } = new MpSslTcpServer(null);

        public static string EofToken { get; set; } = "<EOF>";

        public MpDeviceEndpoint ThisEndpoint { get; set; }
        public MpDeviceEndpoint OtherEndpoint { get; set; }

        //public MpSocketClient SocketClient { get; set; }
        //public MpSocketListener SocketListener { get; set; }

        public MpSessionManager SessionManager { get; set; }

        public string StatusLog { get; set; }

        public MpSyncMesageType LastSyncState { get; set; } = MpSyncMesageType.None;

        //public bool IsClient => SocketClient != null;
        #endregion

        #region Public Methods
        public void Init(MpISync localSync) {
            Task.Run(async () => {
                _localSyncData = localSync;
                while (!MpHelpers.Instance.IsConnectedToNetwork()) {
                    Thread.Sleep(10000);
                }
                ThisEndpoint = MpDeviceEndpointFactory.CreateEndpoint(localSync);

                var sw = new Stopwatch();
                sw.Start();
                MpConsole.WriteLine(@"Attempting Listener discovery...");

                //var privateClient = await MpPrivateEndpointDiscoveryHelper.Discover(
                //    ThisEndpoint.PrivateIp4Address,
                //    ThisEndpoint.PrivatePortNum, 
                //    localSync.IsWpf());
                sw.Stop();
                MpConsole.WriteLine(@"Private ip sweep took {0} ms" + sw.ElapsedMilliseconds);
                StreamWriter w = null;
                StreamReader r = null;
                if (localSync.IsWpf()) {
                   

                    // let's go!
                    //server.Start(IPAddress.Parse("192.168.43.209"), ThisEndpoint.PrivatePortNum);

                    // once a client has connected...
                    //server.Send("[ClientIp:Port]", "Hello, world!");
                    //var listener = new MpSocketListenerAsync(ThisEndpoint,_localSyncData);
                    //listener.OnConnect += Listener_OnConnect;
                    //listener.OnDisconnect += Listener_OnDisconnect;
                    //listener.StartListening();
                    //var tcpl = new TcpListener(IPAddress.Parse("192.168.43.209"), ThisEndpoint.PrivatePortNum);
                    //tcpl.Server.Blocking = true;
                    //tcpl.Start();
                    //var tcpc = tcpl.AcceptTcpClient(); 
                    //using NetworkStream networkStream = tcpc.GetStream();
                    //networkStream.ReadTimeout = 2000;
                    //w = new StreamWriter(networkStream);
                    //w.AutoFlush = true;
                    //string lep = ThisEndpoint.SerializeDbObject();
                    //w.WriteLine(lep);
                    //w.Flush();

                    //r = new StreamReader(tcpc.GetStream());
                    //string cepstr = r.ReadToEnd();
                    //OtherEndpoint = MpDeviceEndpoint.Parse(cepstr);
                    //return;
                } else {
                    
                    //var tcpc = new TcpClient();
                    //tcpc.Connect("192.168.43.209", ThisEndpoint.PrivatePortNum);
                    //using NetworkStream networkStream = tcpc.GetStream();
                    //networkStream.ReadTimeout = 2000;
                    //r = new StreamReader(networkStream);
                    //string sepstr = r.ReadToEnd();
                    //OtherEndpoint = MpDeviceEndpoint.Parse(sepstr);

                    //w = new StreamWriter(networkStream);
                    //w.AutoFlush = true;
                    //string cep = ThisEndpoint.SerializeDbObject();
                    //w.WriteLine(cep);
                    //w.Flush();
                    //return;
                    //OtherEndpoint = new MpDeviceEndpoint() {
                    //    PublicIp4Address = @"192.168.43.209",
                    //    PrivateIp4Address = @"192.168.43.209",
                    //    DeviceGuid = "???",
                    //    PublicPortNum = localSync.GetSyncPort(),
                    //    PrivatePortNum = localSync.GetSyncPort(),
                    //    AccessToken = "???"
                    //};
                    //var client = new MpSocketClientAsync(ThisEndpoint, OtherEndpoint,_localSyncData);
                    //client.OnConnect += Client_OnConnect;
                    //client.StartClient();
                    //return;

                }
            });
        }

        private async void Listener_OnDisconnect(object sender, DateTime e) {
            await Task.Delay(3000);
            (sender as MpSocketListenerAsync).FinishSync();
        }

        private void Client_OnConnect(object sender, string e) {
            var client = (MpSocketClientAsync)sender;
            var listenerEndpoint = MpDeviceEndpoint.Parse(e);
            string msg = string.Empty;

            Task.Run(async () => {
                //Send last sync datetime
                var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(listenerEndpoint.DeviceGuid);
                client.Send(lastSyncDateTime.ToString());

                //receive db log from listener
                msg = client.Receive();

                //send db obj request to listener
                var neededItemList = await _localSyncData.ProcessRemoteDbLog(msg);
                client.Send(neededItemList.ToString());

                //receive db objects from listener
                msg = client.Receive();
                MpConsole.WriteLine(@"Objects from listener: " + msg);
                return;

                var sdt = client.FinishSync();
                MpConsole.WriteLine(@"Sync completed: " + sdt.ToString());
            });
        }

        private void Listener_OnConnect(object sender, string e) {
            var listener = (MpSocketListenerAsync)sender;
            var clientEndpoint = MpDeviceEndpoint.Parse(e);
            string msg = string.Empty;

            Task.Run(async () => {
                // receive client last sync for with listener's deviceGUid
                msg = listener.Receive();
                var clientLastSyncDateTime = DateTime.Parse(msg);
                var localSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                DateTime minLastSyncDateTime = DateTime.MinValue;
                if (localSyncDateTime > clientLastSyncDateTime) {
                    minLastSyncDateTime = clientLastSyncDateTime;
                } else {
                    minLastSyncDateTime = localSyncDateTime;
                }

                //send dblog for device guid after last sync datetime
                var localLogFromLastSync = await _localSyncData.GetLocalLog(minLastSyncDateTime);
                listener.Send(localLogFromLastSync);

                //receive dbo request guid/table name pairs that client needs
                msg = listener.Receive();
                MpConsole.WriteLine(@"Client db request: " + msg);
                return;
            });
            return;
        }

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        protected string OpenMessage(string e) {
            if (!ValidateMessagee(e)) {
                throw new Exception("Message either has a bad check sum or no <Eof>");
            }
            return e.Replace(MpSocket.EofToken, string.Empty);
        }

        protected bool ValidateMessagee(string msg) {
            if (msg == null) {
                return false;
            }
            return msg.Contains(MpSocket.EofToken);
        }
        #endregion
    }
}
