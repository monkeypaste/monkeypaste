using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyzeOutput : MpAvActionOutput {
        public override object OutputData => NewCopyItem == null ? TransactionResult : NewCopyItem;
        public MpAnalyzerPluginResponseFormat TransactionResult { get; set; }
        public MpCopyItem NewCopyItem { get; set; }
        public override string ActionDescription {
            get {
                //return $"Result of analysis of CopyItem({CopyItem.Id},{CopyItem.Title}) was: " + Environment.NewLine + NewCopyItem.ToStringOrDefault();
                return $"Result of analysis: '{OutputData.ToStringOrEmpty()}'";
            }
        }

    }
}
