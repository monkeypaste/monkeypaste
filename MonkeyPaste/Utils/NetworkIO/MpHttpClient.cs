using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpHttpClient : MpIAsyncSingletonViewModel<MpHttpClient> {
        #region Properties

        public HttpClient HttpClient { get; private set; }

        #endregion

        #region Constructors

        private static MpHttpClient _instance;
        public static MpHttpClient Instance => _instance ?? (_instance = new MpHttpClient());

        public async Task InitAsync() {
            await Task.Delay(1);
        }

        public MpHttpClient() {
            HttpClient = new HttpClient();
        }

        #endregion
    }
}
