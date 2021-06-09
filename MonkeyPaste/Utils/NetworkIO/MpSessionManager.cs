using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MonkeyPaste {
    public class MpSessionManager : IDisposable {
        #region Singleton
        private static readonly Lazy<MpSessionManager> _Lazy = new Lazy<MpSessionManager>(() => new MpSessionManager());
        public static MpSessionManager Instance { get { return _Lazy.Value; } }

        private MpSessionManager() {
            // Register for connectivity changes, be sure to unsubscribe when finished
            Connectivity.ConnectivityChanged += MpSessionManager_ConnectivityChanged;
        }       

        
        #endregion

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
        #endregion

        #region Properties
        public string ControllerName { get; set; } = @"UserSessions";

        public string ConnectAction { get; set; } = @"Create";

        public string DisconnectAction { get; set; } = @"Delete";

        public bool IsConnected => !string.IsNullOrEmpty(_accessToken);
        #endregion

        #region Public Methods
        public bool Init() {
            if(!MpHelpers.Instance.IsConnectedToInternet()) {
                MpConsole.WriteLine("*************** NO INTERNET CONNECTION AVAILABLE **********************");
                return false;
            }
            //if (!MpHelpers.Instance.IsMpServerAvailable()) {
            //    MpConsole.WriteLine("*************** HOST SERVER NOT AVAILABLE **********************");
            //    return false;
            //}
            

            Task.Run(async () => {
                Uri uri = new Uri(
                    string.Format(@"{0}/{1}/{2}?email={3}&ip={4}",
                    MpPreferences.SyncServerEndpoint,
                    ControllerName,
                    ConnectAction,
                    MpPreferences.UserEmail,
                    MpHelpers.Instance.GetUserIp4Address()
                    ));
                //var connStr = $"https://192.168.43.209:44376/UserSessions?email={MpPreferences.UserEmail}&ip={MpHelpers.Instance.GetUserIp4Address()}";
                //var uri = new Uri(connStr);
                try {
                    HttpResponseMessage response = await _client.GetAsync(uri);
                    if (response.IsSuccessStatusCode) {
                        var byteArray = await response.Content.ReadAsByteArrayAsync();
                        _accessToken = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                        MpConsole.WriteLine(@"Session access token: " + _accessToken);
                    } else {
                        _accessToken = string.Empty;
                        MpConsole.WriteTraceLine(@"Could not connect server: " + uri.ToString());
                    }
                }
                catch(Exception ex) {
                    _accessToken = string.Empty;
                    MpConsole.WriteTraceLine("", ex);
                }
            });
            return true;
        }

        public void Dispose() {
            throw new NotImplementedException();
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
