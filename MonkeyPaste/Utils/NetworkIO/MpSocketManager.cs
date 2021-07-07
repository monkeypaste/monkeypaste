using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace MonkeyPaste {
    public class MpSocketManager : IDisposable {
        #region Private Variables
        private int _upPort = 44376;
        private int _downPort = 44377;

        private string _accessToken = string.Empty;
        #endregion

        #region Properties
        public MpSocketClientAsync SocketClient { get; set; }
        public MpSocketListenerAsync SocketListener { get; set; }

        public MpSessionManager SessionManager { get; set; }

        public ObservableCollection<MpDeviceEndpoint> AvailableEndPoints { get; set; } = new ObservableCollection<MpDeviceEndpoint>();

        public MpDeviceEndpoint ThisEndPoint { get; set; } = null;

        public MpDeviceEndpoint HostEndPoint {
            get {
                if (AvailableEndPoints.Count > 0) {
                    return AvailableEndPoints[0];
                }
                return null;
            }
        }

        public bool CanSync => ThisEndPoint != null && AvailableEndPoints.Count > 1;

        public bool IsHost => ThisEndPoint == HostEndPoint;

        public bool IsConnected => ThisEndPoint != null && !string.IsNullOrEmpty(ThisEndPoint.AccessToken);

        public MpCertificateManager CertManager { get; set; }
        #endregion

        #region Public Methods
        public MpSocketManager(string pubip, string privip) {
            //ThisEndPoint = new MpDeviceEndpoint(pubip, privip, _upPort,);

            //SessionManager = new MpSessionManager(_thisIp, _syncPort);

            //var deviceData = connectResponse.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 0; i < deviceData.Length; i += 4) {
            //    var ep = new MpDeviceEndpoint(
            //            deviceData[i],
            //            Convert.ToInt32(deviceData[i + 1]),
            //            deviceData[i + 2],
            //            DateTime.Parse(deviceData[i + 3]));
            //    AvailableEndPoints.Add(ep);

            //    if (ep.AccessToken == _accessToken) {
            //        ThisEndPoint = ep;
            //    }
            //}
            //AvailableEndPoints.Add(ThisEndPoint);

            //MpConsole.WriteLine(@"Session access token: " + _accessToken);
        }

        public void Dispose() {
            SocketClient.Dispose();
            SocketListener.Dispose();
        }

        public async Task<bool> Disconnect() {
            if (!IsConnected) {
                return true;
            }
            //var uri = new Uri(
            //        string.Format(@"https://www.monkeypaste.com/api/disconnect.php?email={0}&ip={1}&at={2}",
            //        @"test@test.com",
            //        _ip,
            //        ThisEndPoint.AccessToken
            //        ));
            try {
                //var response = await _client.GetAsync(uri);
                //if (response.IsSuccessStatusCode) {
                //    MpConsole.WriteLine(@"Closed sync session");
                //    return true;
                //} else {
                //    MpConsole.WriteTraceLine(@"Could not close session");
                //    return false;
                //}
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("", ex);
                return false;
            }
            return false;
        }
        #endregion

        #region Private Methods
        private async void EndPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (!IsConnected) {
                return;
            }
            if (IsHost) {
                //this device will become the server since its connected and 
                //there are no other clients
                //SocketListener.StartListening(_syncPort);
                //SocketListener.Start(ThisEndPoint);
            } else if (CanSync) {
                //SocketClient.Send(HostEndPoint, "Hi Server!");

                //var cil = await _localSyncData.GetLocalData();
                //foreach (var c in cil) {
                //    SocketClient.Send(c.ToString());
                //}
            }
        }
        #endregion


    }
}
