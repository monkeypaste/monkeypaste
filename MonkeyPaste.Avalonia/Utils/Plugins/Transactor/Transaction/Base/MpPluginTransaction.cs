using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste.Avalonia {
    public abstract class MpPluginTransactionBase {
        public DateTime RequestTime { get; set; }
        public DateTime? ResponseTime { get; set; }

        public MpPluginParameterRequestFormat Request { get; set; }
        public MpAnalyzerPluginResponseFormat Response { get; set; }

        public string TransactionErrorMessage { get; set; }
    }
}
