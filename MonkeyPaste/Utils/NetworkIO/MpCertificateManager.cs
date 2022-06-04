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
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpCertificateManager {
        private X509Certificate2 _cert = null;

        public MpCertificateManager() {
            if (DateTime.UtcNow > MpPreferences.SslCertExpirationDateTime) {
                CreateCertificate();
            }
            _cert = LoadCertificate(MpPreferences.SyncCertPath);

            if (_cert == null) {
                MpConsole.WriteTraceLine(@"Error could not load sync certificate");
            }
        }

        private void CreateCertificate() {
            AsymmetricKeyParameter caPrivKey = GenerateCACertificate(MpPreferences.SslCASubject);

            var cert = GenerateSelfSignedCertificate(MpPreferences.SslCertSubject, MpPreferences.SslCASubject, caPrivKey);

            MpPreferences.SslPublicKey = cert.GetPublicKeyString();

            SaveCertificate(MpPreferences.SyncCertPath, cert);
        }

        private AsymmetricKeyParameter GenerateCACertificate(string subjectName, int keyStrength = 2048) {
            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(Org.BouncyCastle.Math.BigInteger.One, Org.BouncyCastle.Math.BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);


            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate            

            var issuerKeyPair = subjectKeyPair;
            const string signatureAlgorithm = "SHA256WithRSA";
            //var signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerKeyPair.Private);
            //var bouncyCert = certificateGenerator.Generate(signatureFactory);

            // Signature Algorithm
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);
            var bouncyCert = certificateGenerator.Generate(issuerKeyPair.Private, random); //signatureFactory);

            // Lets convert it to X509Certificate2
            X509Certificate2 certificate;

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.SetKeyEntry($"{subjectName}_key", new AsymmetricKeyEntry(subjectKeyPair.Private), new[] { new X509CertificateEntry(bouncyCert) });
            string exportpw = Guid.NewGuid().ToString("x");

            using (var ms = new System.IO.MemoryStream()) {
                store.Save(ms, exportpw.ToCharArray(), random);
                certificate = new X509Certificate2(ms.ToArray(), exportpw, X509KeyStorageFlags.Exportable);
            }
            //signature algorithm
            //ISignatureFactory signatureFactory = new Asn1SignatureFactory(MpPreferences.SslAlgorithm, issuerKeyPair.Private, random);

            // Selfsign certificate
            //var certificate = certificateGenerator.Generate(issuerKeyPair.Private, random); //signatureFactory);
            //var x509 = new X509Certificate2(certificate.GetEncoded(),string.Empty);

            // Add CA certificate to Root store
            SaveCertificate(MpPreferences.SyncCaPath, certificate);

            return issuerKeyPair.Private;
        }

        private X509Certificate2 GenerateSelfSignedCertificate(
            string subjectName,
            string issuerName,
            AsymmetricKeyParameter issuerPrivKey,
            int keyStrength = 2048) {
            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(Org.BouncyCastle.Math.BigInteger.One, Org.BouncyCastle.Math.BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Signature Algorithm
            //const string signatureAlgorithm = "SHA256WithRSA";            
            //certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);

            //ISignatureFactory signatureFactory = new Asn1SignatureFactory(MpPreferences.SslAlgorithm, issuerPrivKey, random);


            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = new X509Name(issuerName);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            MpPreferences.SslCertExpirationDateTime = notAfter;

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            var issuerKeyPair = subjectKeyPair;

            // Corresponding private key
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);

            
            const string signatureAlgorithm = "SHA256WithRSA";
            //var signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, issuerKeyPair.Private);
            //var bouncyCert = certificateGenerator.Generate(signatureFactory);

            // Signature Algorithm
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);
            var bouncyCert = certificateGenerator.Generate(issuerKeyPair.Private, random); //signatureFactory);

            // Lets convert it to X509Certificate2
            X509Certificate2 certificate;

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.SetKeyEntry($"{subjectName}_key", new AsymmetricKeyEntry(subjectKeyPair.Private), new[] { new X509CertificateEntry(bouncyCert) });
            string exportpw = Guid.NewGuid().ToString("x");

            using (var ms = new System.IO.MemoryStream()) {
                store.Save(ms, exportpw.ToCharArray(), random);
                certificate = new X509Certificate2(ms.ToArray(), exportpw, X509KeyStorageFlags.Exportable);
            }

            // Merge into X509Certificate2
            //var x509 = new X509Certificate2(certificate.GetEncoded());

            var seq = (Asn1Sequence)Asn1Object.FromByteArray(info.PrivateKeyData.GetDerEncoded());
            if (seq.Count != 9)
                throw new PemException("malformed sequence in RSA private key");


            var rsa = RsaPrivateKeyStructure.GetInstance(seq);
            RsaPrivateCrtKeyParameters rsaparams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);

            certificate.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
            return certificate;
        }


        // You can also load the certificate from to CurrentUser store
        private X509Certificate2 LoadCertificate(string path) {
            try {
                var bytes = File.ReadAllBytes(path);
                var cert = new X509Certificate2(bytes);
                return cert;
            }
            catch(Exception ex) {
                MpConsole.WriteTraceLine(@"Error loading cert at path: " + path, ex);
            }
            //var userStore = new X509Store(st, sl);
            //userStore.Open(OpenFlags.OpenExistingOnly);
            //var collection = userStore.Certificates.Find(X509FindType.FindBySubjectName, Environment.MachineName, false);
            //if (collection.Count > 0) {
            //    return collection[0];
            //}
            //userStore.Close();
            return null;
        }

        private bool SaveCertificate(string path, X509Certificate2 cert) {
            try {
                // Export the certificate including the private key.
                byte[] certBytes = cert.Export(X509ContentType.Pkcs12);
                File.WriteAllBytes(path, certBytes);

                //X509Store store = new X509Store(st, sl);
                //store.Open(OpenFlags.ReadWrite);
                //store.Add(cert);

                //store.Close();
                return true;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Error writing cert to path: "+path, ex);
            }
            return false;
        }

        //public bool RemoveCertFromStore(
        //    System.Security.Cryptography.X509Certificates.X509Certificate2 cert,
        //    System.Security.Cryptography.X509Certificates.StoreName st,
        //    System.Security.Cryptography.X509Certificates.StoreLocation sl) {
        //    try {
        //        X509Store store = new X509Store(st, sl);
        //        store.Open(OpenFlags.ReadWrite);
        //        store.Remove(cert);

        //        store.Close();
        //        return true;
        //    }
        //    catch (Exception ex) {
        //        MpConsole.WriteTraceLine("", ex);
        //    }
        //    return false;
        //}
    }
}
