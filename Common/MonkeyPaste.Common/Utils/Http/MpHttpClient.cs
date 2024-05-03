using System;
using System.Net.Http;
using System.Net.Security;
namespace MonkeyPaste.Common {

    public class MpHttpClient {
        // from https://stackoverflow.com/a/75686081/105028
        public static HttpClient Client { get; private set; }

        static MpHttpClient() {
            var sslOptions = new SslClientAuthenticationOptions {
                // from https://dev.to/tswiftma/switching-from-httpclienthandler-to-socketshttphandler-17h3
                // Leave certs unvalidated for debugging
                RemoteCertificateValidationCallback = delegate { return true; },
            };
            var handler = new StandardSocketsHttpHandler {
                // Sets how long a connection can be in the pool to be considered reusable (by default - infinite)
                ConnectTimeout = TimeSpan.FromSeconds(10),
                PooledConnectionLifetime = TimeSpan.FromMinutes(1),
                SslOptions = sslOptions
            };

            Client = new HttpClient(handler, disposeHandler: false);
            Client.SetDefaultUserAgent();
        }
    }
}
