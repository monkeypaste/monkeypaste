using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste {
    public abstract class MpPluginTransactionBase {
        public DateTime RequestTime { get; set; }
        public DateTime? ResponseTime { get; set; }

        public MpPluginRequestFormatBase Request { get; set; }
        public MpPluginResponseFormatBase Response { get; set; }

        public string TransactionErrorMessage { get; set; }
    }
}
