using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
//using Xamarin.Essentials;

namespace MonkeyPaste {
    public class MpHttpsValidation : IDisposable {


        private string _serverPublicKey = string.Empty;
        private Uri _serverUri = null;



        //Call GenerateSSLpubklickey callback method and repalce here   
        public MpHttpsValidation(Uri serverUri, string serverPublicKey) {
            if (!MpNetworkHelpers.IsConnectedToNetwork()) {
                Dispose();
            }

            _serverUri = serverUri;
            _serverPublicKey = serverPublicKey;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // ServicePointManager.ServerCertificateValidationCallback = OnValidateCertificate;  
            //Generate Public Key and replace publickey variable   
            //  ServicePointManager.ServerCertificateValidationCallback = GenerateSSLPublicKey;  
            ServicePointManager.ServerCertificateValidationCallback = OnValidateCertificate;
        }

        private bool OnValidateCertificate(
            object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors) {
            var certPublicString = certificate?.GetPublicKeyString();
            var keysMatch = _serverPublicKey == certPublicString;
            return keysMatch;
        }

        private string GenerateSSLPublicKey(
            object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors) {
            string certPublicString = certificate?.GetPublicKeyString();
            return certPublicString;
        }

        public void Dispose() {

        }
    }
}
