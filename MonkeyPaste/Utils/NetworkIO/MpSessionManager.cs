using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public class MpSessionManager {
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

        #endregion

        #region Public Methods

        public async Task Disconnect(MpDeviceEndpoint cep, bool disconnectAll = false, int tryCount = 5) {
            //var uri = new Uri(
            //            string.Format(@"https://www.monkeypaste.com/api/disconnect.php?email={0}&at={1}&clearAll={2}",
            //            @"test@test.com",
            //            cep.PrimaryPrivateIp4Address,
            //            cep.AccessToken,
            //            disconnectAll ? "1":"0"
            //            ));
            try {
                var uri = new Uri(string.Format(@"https://www.monkeypaste.com/api/disconnect.php"));
                var data = new FormUrlEncodedContent(
                        new Dictionary<string, string>() {
                            { "email", "test@test.com" },
                            { "at", cep.AccessToken },
                            { "clearAll", disconnectAll ? "1":"0" }
                        });
                var response = await _client.PostAsync(uri, data);
                if (response.IsSuccessStatusCode) {
                    var result = await response.Content.ReadAsStringAsync();
                    MpConsole.WriteLine(@"Disconnected local client");
                } else {
                    MpConsole.WriteTraceLine(@"Failed to disconnect");
                    await Disconnect(cep, disconnectAll, tryCount--);
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Exception disconnecting session: " + ex);
                await Disconnect(cep, disconnectAll, tryCount--);
            }
        }

        public async Task<string> Connect(MpDeviceEndpoint cep, int tryCount = 5) {
            if(tryCount < 0) {
                return string.Empty;
            }
            //var uri = new Uri(
            //        string.Format(@"https://www.monkeypaste.com/api/connect.php?email={0}&passhash={1}&ip={2}&privipl={3}&primIpIdx={4}&port={5}&at={6}&dguid={7}",
            //        @"test@test.com",
            //        @"password",
            //        cep.PublicIp4Address,
            //        cep.PrivateIp4AddressesCsv,
            //        cep.PrimaryPrivateIp4AddressIdx,
            //        cep.PrivatePortNum,
            //        cep.AccessToken,
            //        cep.DeviceGuid
            //        ));
            var uri = new Uri(string.Format(@"https://www.monkeypaste.com/api/connect.php"));
            try {
                var data = new FormUrlEncodedContent(
                        new Dictionary<string, string>() {
                            { "email", "test@test.com" },
                            { "passhash", "password" },
                            { "ip", cep.PublicIp4Address },
                            { "privipl", cep.PrivateIp4AddressesCsv },
                            { "primIpIdx", cep.PrimaryPrivateIp4AddressIdx.ToString() },
                            { "port", cep.PrivatePortNum.ToString() },
                            { "at", cep.AccessToken },
                            { "dguid", cep.DeviceGuid }
                        });
                var response = await _client.PostAsync(uri,data);
                var result = await response.Content.ReadAsStringAsync();
                return result;
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
