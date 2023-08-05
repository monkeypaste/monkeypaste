using System;

namespace MonkeyPaste {
    public interface MpISslInfo {
        DateTime SslCertExpirationDateTime { get; set; }
        string SyncCertPath { get; }
        string SslCertSubject { get; }
        string SyncCaPath { get; }
        string SslCASubject { get; set; }
        string SslPublicKey { get; set; }
    }
}
