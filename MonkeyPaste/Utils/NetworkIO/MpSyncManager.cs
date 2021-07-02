using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSyncManager : IDisposable {
        #region Singleton
        private static readonly Lazy<MpSyncManager> _Lazy = new Lazy<MpSyncManager>(() => new MpSyncManager());
        public static MpSyncManager Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Variables

        private MpISyncData _localSyncData;
        
        #endregion

        #region Properties
        //public MpSocketClient SocketClient { get; set; } = new MpSocketClient();
        //public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        //public MpSslTcpClient SslClient { get; set; } = new MpSslTcpClient(null);
        //public MpSslTcpServer SslServer { get; set; } = new MpSslTcpServer(null);

        public MpDeviceEndpoint ThisEndpoint { get; set; }
        public MpDeviceEndpoint ServerEndpoint { get; set; }

        public MpSocketClient SocketClient { get; set; }
        public MpSocketListener SocketListener { get; set; }

        public MpSessionManager SessionManager { get; set; }

        public string StatusLog { get; set; }
        #endregion

        #region Public Methods
        public void Init(MpISyncData localSyncData, bool isListener) {
            _localSyncData = localSyncData;
            Task.Run(async () => {
                if (isListener) {
                    ThisEndpoint = new MpDeviceEndpoint(
                        @"192.168.43.209",
                        @"192.168.43.209",
                        44376,
                        MpHelpers.Instance.GetNewAccessToken(),
                        DateTime.Now);

                    SocketListener = MpSocketListener.CreateListener(ThisEndpoint,
                        (s, e) => {
                            Console.WriteLine(@"Sent: " + e);
                            SocketClient = (MpSocketClient)s;
                        },
                        (s, e) => {
                            Console.WriteLine(@"Received: " + e);
                        },
                        (s, e) => {
                            Console.WriteLine(@"Error: " + e);
                        });
                    SocketListener.Start2(localSyncData);
                } else {
                    //ThisEndpoint = new MpDeviceEndpoint(
                    //    @"192.168.43.1",
                    //    @"192.168.43.1",
                    //    44376,
                    //    MpHelpers.Instance.GetNewAccessToken(),
                    //    DateTime.Now);

                    //ServerEndpoint = new MpDeviceEndpoint(
                    //    @"192.168.43.209",
                    //    @"192.168.43.209",
                    //    44376,
                    //    MpHelpers.Instance.GetNewAccessToken(),
                    //    DateTime.Now);

                    //SocketClient = MpSocketClient.CreateClient(
                    //ServerEndpoint, ThisEndpoint,
                    //    (s, e) => {
                    //        Console.WriteLine(@"Sent: " + e);
                    //        SocketClient = (MpSocketClient)s;
                    //    },
                    //    (s, e) => {
                    //        Console.WriteLine(@"Received: " + e);
                    //    },
                    //    (s, e) => {
                    //        Console.WriteLine(@"Error: " + e);
                    //    }
                    //);
                    //SocketClient.Send(@"Hi Server!");

                    // Process the data sent by the client.

                    var sdata = await localSyncData.GetLocalData();
                    var data = localSyncData.ConvertToJson(sdata);//data.ToUpper();

                    MpSocketClient.Connect(@"192.168.43.209", data);
                }



                //_syncPort = 443;

                //SocketListener.OnReceive += SocketListener_OnReceive;
                //SocketClient.OnReceive += SocketClient_OnReceive;

                //

                //SessionManager.OnConnected += async (s,e) => {
                //    var data = _localSyncData.GetLocalData();
                //    await SessionManager.Upload(data);
                //};
                //SessionManager.AvailableEndPoints.CollectionChanged += EndPoints_CollectionChanged; 
            });
                       
        }

        public void Upload() {

        }

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        
        #region Event Handlers
        

        private void SocketClient_OnReceive(object sender, string e) {
            throw new NotImplementedException();
        }

        private void SocketListener_OnReceive(object sender, string e) {
            throw new NotImplementedException();
        }
        #endregion
        #endregion
    }
}
