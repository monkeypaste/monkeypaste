using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Xamarin.Essentials;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace MonkeyPaste {
    public class MpHttpsValidation : IDisposable {


        private string _serverPublicKey = string.Empty;
        private Uri _serverUri = null;



        //Call GenerateSSLpubklickey callback method and repalce here   
        public MpHttpsValidation(Uri serverUri, string serverPublicKey) {
            if(!MpNetworkHelpers.IsConnectedToNetwork()) {
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
