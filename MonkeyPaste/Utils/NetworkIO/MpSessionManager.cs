using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public class MpSessionManager : IDisposable {
        #region Private Variables 
        private HttpClient _client {
            get {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                return client;
            }
        }

        private string _accessToken = string.Empty;

        private string _ip;
        private int _port;
        #endregion

        #region Properties
        public ObservableCollection<MpDeviceEndpoint> AvailableEndPoints { get; set; } = new ObservableCollection<MpDeviceEndpoint>();

        public MpDeviceEndpoint ThisEndPoint { get; set; } = null;

        public MpDeviceEndpoint HostEndPoint {
            get {
                if(AvailableEndPoints.Count > 0) {
                    return AvailableEndPoints[0];
                }
                return null;
            }
        }

        public bool CanSync => ThisEndPoint != null && AvailableEndPoints.Count > 1;

        public bool IsHost => ThisEndPoint == HostEndPoint;

        public bool IsConnected => ThisEndPoint != null && !string.IsNullOrEmpty(ThisEndPoint.AccessToken);
        #endregion

        #region Public Methods
        public MpSessionManager(string ip, int port = 44376) {
            _ip = ip;
            _port = port;

#if __MOBILE__
// Register for connectivity changes, be sure to unsubscribe when finished
            Connectivity.ConnectivityChanged += MpSessionManager_ConnectivityChanged;
#endif

            Task.Run(Initialize);
        }

        public async Task<bool> Disconnect() {
            if(!IsConnected) {
                return true;
            }
            var uri = new Uri(
                    string.Format(@"https://www.monkeypaste.com/api/disconnect.php?email={0}&ip={1}&at={2}",
                    @"test@test.com",
                    _ip,
                    ThisEndPoint.AccessToken
                    ));
            try {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode) {                    
                    MpConsole.WriteLine(@"Closed sync session");
                    return true;
                } else {
                    MpConsole.WriteTraceLine(@"Could not close session");
                    return false;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("", ex);
                return false;
            }
        }

        public void Dispose() {
            Disconnect();
            AvailableEndPoints.Clear();
            ThisEndPoint = null;
        }
        #endregion

        #region Private Methods

        private async Task Initialize() {
            var at = MpHelpers.Instance.GetNewAccessToken();
            
            var uri = new Uri(
                    string.Format(@"https://www.monkeypaste.com/api/connect.php?email={0}&ip={1}&port={2}&at={3}",
                    @"test@test.com",
                    _ip,
                    _port,
                    at
                    ));
            try {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode) {
                    _accessToken = at;
                    var result = await response.Content.ReadAsStringAsync();// ReadAsByteArrayAsync();

                    var deviceData = result.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < deviceData.Length; i += 3) {
                        AvailableEndPoints.Add(
                            new MpDeviceEndpoint(
                                deviceData[i],
                                Convert.ToInt32(deviceData[i + 1]),
                                deviceData[i + 2]));
                    }
                    ThisEndPoint = new MpDeviceEndpoint(_ip, _port, at);
                    AvailableEndPoints.Add(ThisEndPoint);
                    MpConsole.WriteLine(@"Session access token: " + _accessToken);
                } else {
                    _accessToken = string.Empty;
                    MpConsole.WriteTraceLine(@"Could not connect server: " + uri.ToString());
                }
            }
            catch (Exception ex) {
                _accessToken = string.Empty;
                MpConsole.WriteTraceLine("", ex);
            }
        }

        #region Event Handlers

        private void MpSessionManager_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
            var access = e.NetworkAccess;
            var profiles = e.ConnectionProfiles;
        }

        #endregion

        #endregion
    }
}
