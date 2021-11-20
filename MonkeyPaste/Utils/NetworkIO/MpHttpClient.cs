using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MonkeyPaste {
    public class MpHttpClient : MpSingleton<MpHttpClient> {
        #region Properties

        public HttpClient HttpClient { get; private set; }

        #endregion

        #region Constructors

        public MpHttpClient() {
            HttpClient = new HttpClient();
        }

        #endregion
    }
}
