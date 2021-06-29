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


        private string _ip;
        private int _port;
        #endregion

        #region Properties

        public MpSocketManager SocketManager { get; set; }
        #region Events
        public event EventHandler<string> OnConnected;
        #endregion
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
            if(SocketManager == null) {
                return true;
            }
            return await SocketManager.Disconnect();
        }

        //public async Task Upload(object data) {
        //    var dataItems = (List<object>)data;
        //    var baseUrl = @"https://www.monkeypaste.com/api/upload.php?at={0}&data={1}&groupId={2}&packetId={3}";

        //    for (int i = 0; i < dataItems.Count; i++) {
        //        var uri = new Uri(
        //            string.Format(baseUrl,
        //            _accessToken,
        //            dataItems[i].ToString(),
        //            i,
        //            0));
        //        try {
        //            var response = await _client.GetAsync(uri);
        //            if (response.IsSuccessStatusCode) {
        //                continue;
        //            } else {
        //                Console.WriteLine("Failure");
        //            }
        //        }
        //        catch (Exception ex) {
        //            Console.WriteLine("Failure: " + ex);
        //        }
        //    } 
        //}

        public void Dispose() {
            Disconnect();
            SocketManager.Dispose();
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
                    //OnConnected?.Invoke(this, at);
                    //_accessToken = at;
                    var result = await response.Content.ReadAsStringAsync();// ReadAsByteArrayAsync();
                    SocketManager = new MpSocketManager(result, at);   
                    //_accessToken = string.Empty;
                    MpConsole.WriteTraceLine(@"Could not connect server: " + uri.ToString());
                }
            }
            catch (Exception ex) {
                //_accessToken = string.Empty;
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
