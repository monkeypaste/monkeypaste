using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSessionManager {
        #region Singleton
        private static readonly Lazy<MpSessionManager> _Lazy = new Lazy<MpSessionManager>(() => new MpSessionManager());
        public static MpSessionManager Instance { get { return _Lazy.Value; } }
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
        public string ServerIpOrHostname { get; set; } = @"192.168.43.209";

        public int ServerPortNum { get; set; } = 44380;

        public string ControllerName { get; set; } = @"MpUsers";

        public string ConnectAction { get; set; } = @"Connect";

        public string DisconnectAction { get; set; } = @"Disconnect";
        #endregion

        #region Public Methods
        public bool Init(string email) {
            if(!MpHelpers.Instance.CheckForInternetConnection()) {
                return false;
            }

            Task.Run(async () => {
                Uri uri = new Uri(
                    string.Format(@"https://{0}:{1}/{2}/{3}?email={4}&ip={5}", 
                    ServerIpOrHostname, 
                    ServerPortNum,
                    ControllerName,
                    ConnectAction,
                    email,
                    MpHelpers.Instance.GetUserIp4Address()
                    ));

                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode) {
                    var byteArray = await response.Content.ReadAsByteArrayAsync();
                    _accessToken = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);

                    MpConsole.WriteLine(@"Session access token: " + _accessToken);
                } else {
                    MpConsole.WriteTraceLine(@"Could not connect server: " + uri.ToString());
                }
            });
            return true;
        }
        #endregion
    }
}
