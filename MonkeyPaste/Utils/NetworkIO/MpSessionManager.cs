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
        #endregion

        #region Properties
        #region Events
        #endregion
        #endregion

        #region Public Methods

        public async Task<bool> Disconnect() {
            return true;
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
        }

        public async Task<string> Connect(MpDeviceEndpoint cep) {
            var uri = new Uri(
                    string.Format(@"https://www.monkeypaste.com/api/connect.php?email={0}&passhash={1}&ip={2}&privip={3}port={4}&at={5}",
                    @"test@test.com",
                    @"passhash",
                    cep.PublicIp4Address,
                    cep.PrivateIp4Address,
                    cep.PublicConnectPortNum,
                    cep.AccessToken
                    ));
            try {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode) {
                    var result = await response.Content.ReadAsStringAsync();// ReadAsByteArrayAsync();                    
                    MpConsole.WriteTraceLine(@"Connected to server with access token: " + uri.ToString());
                    return result;
                } else {
                    MpConsole.WriteTraceLine(@"Could not connect server: " + uri.ToString());
                    return string.Empty;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Error connecting to server: " + ex);
                return string.Empty;
            }
        }
        #endregion

        #region Private Methods

        #region Event Handlers

        private void MpSessionManager_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
            var access = e.NetworkAccess;
            var profiles = e.ConnectionProfiles;
        }

        #endregion

        #endregion
    }
}
