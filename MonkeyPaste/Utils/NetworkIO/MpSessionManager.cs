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
using System.Runtime.ConstrainedExecution;
using System.Net.WebSockets;

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

        #region Events
        #endregion

        #endregion

        #region Public Methods

        public async Task Disconnect(MpDeviceEndpoint cep, bool disconnectAll = false, int tryCount = 5) {
            var uri = new Uri(
                        string.Format(@"https://www.monkeypaste.com/api/disconnect.php?email={0}&ip={1}&at={2}&clearAll={3}",
                        @"test@test.com",
                        cep.PrimaryPrivateIp4Address,
                        cep.AccessToken,
                        disconnectAll ? "1":"0"
                        ));
            try {
                var response = await _client.GetAsync(uri);
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
            var uri = new Uri(
                    string.Format(@"https://www.monkeypaste.com/api/connect.php?email={0}&passhash={1}&ip={2}&privipl={3}&primIpIdx={4}&port={5}&at={6}&dguid={7}",
                    @"test@test.com",
                    @"password",
                    cep.PublicIp4Address,
                    cep.PrivateIp4AddressesCsv,
                    cep.PrimaryPrivateIp4AddressIdx,
                    cep.PrivatePortNum,
                    cep.AccessToken,
                    cep.DeviceGuid
                    ));
            try {
                var response = await _client.GetAsync(uri);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Error connecting to server: " + ex);
                return string.Empty;
            }
        }

        public void SendMessage(MpStreamMessage smsg) {
            Task.Run(async () => {
                var uri = new Uri(string.Format(@"https://www.monkeypaste.com/api/sync.php"));
                try {
                    var data = new FormUrlEncodedContent(
                        new Dictionary<string, string>() {
                            { "action", "send" },
                            { "fromGuid",smsg.Header.FromGuid },
                            { "toGuid",smsg.Header.ToGuid },
                            { "smsg", smsg.SerializeDbObject() }
                        });
                    var response = await _client.PostAsync(uri, data);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(@"Error sending message: " + smsg.ToString());
                    MpConsole.WriteTraceLine(@"Error sending message to server exception: " + ex);
                }
            });
        }

        public async Task<MpStreamMessage> ReceiveMessage(MpDeviceEndpoint thisEndpoint) {
            var uri = new Uri(string.Format(@"https://www.monkeypaste.com/api/sync.php"));
            try {
                var data = new FormUrlEncodedContent(
                    new Dictionary<string, string>() {
                            { "action", "receive" },
                            { "fromGuid",thisEndpoint.DeviceGuid }
                    });
                var response = await _client.PostAsync(uri, data);
                if (response.IsSuccessStatusCode) {
                    var result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result)) {
                        return null;
                    }
                    return MpStreamMessage.Parse(result);
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Error receiving message to server exception: " + ex);
            }
            return null;
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
