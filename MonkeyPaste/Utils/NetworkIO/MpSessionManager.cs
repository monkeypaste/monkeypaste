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

        public async Task<List<MpDeviceEndpoint>> Connect(MpDeviceEndpoint cep) {
            var uri = new Uri(
                    string.Format(@"https://www.monkeypaste.com/api/connect.php?email={0}&passhash={1}&ip={2}&privip={3}&port={4}&at={5}&dguid={6}",
                    @"test@test.com",
                    @"password",
                    cep.PublicIp4Address,
                    cep.PrivateIp4Address,
                    cep.PublicConnectPortNum,
                    cep.AccessToken,
                    cep.DeviceGuid
                    ));
            try {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode) {
                    var result = await response.Content.ReadAsStringAsync();                   
                    MpConsole.WriteTraceLine(@"Connected to server with access token: " + uri.ToString());
                    return ProcessWebConnectResponse(result);
                } else {
                    MpConsole.WriteTraceLine(@"Could not connect server: " + uri.ToString());
                    return new List<MpDeviceEndpoint>();
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Error connecting to server: " + ex);
                return new List<MpDeviceEndpoint>();
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

        private List<MpDeviceEndpoint> ProcessWebConnectResponse(string response) {
            var repl = new List<MpDeviceEndpoint>();
            if(string.IsNullOrEmpty(response)) {
                return repl;
            }
            var rpl = response.Split(new string[] { "&" }, StringSplitOptions.None);

            for (int i = 0; i < rpl.Length; i++) {
                if(i + 5 >= rpl.Length) {
                    MpConsole.WriteTraceLine(@"Malformed remote device response from server, ignoring");
                    break;
                }
                //format: "$oip&$oprivip&$oportnum&$oat&$oldt&$odg&";
                var ep = new MpDeviceEndpoint();
                ep.PublicIp4Address = rpl[i];
                ep.PrivateIp4Address = rpl[++i];
                ep.PublicConnectPortNum = ep.PrivateConnectPortNum = Convert.ToInt32(rpl[++i]);
                ep.AccessToken = rpl[++i];
                ep.ConnectDateTime = DateTime.Parse(rpl[++i]);
                ep.DeviceGuid = rpl[++i];
                repl.Add(ep);
            }
            return repl;
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
