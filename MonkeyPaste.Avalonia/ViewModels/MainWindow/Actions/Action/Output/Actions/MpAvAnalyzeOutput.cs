using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyzeOutput : MpAvActionOutput {
        public override object OutputData => NewCopyItem;
        //public object TransactionResult { get; set; }
        public MpCopyItem NewCopyItem { get; set; }
        public override string ActionDescription {
            get {
                return $"Result of analysis of CopyItem({CopyItem.Id},{CopyItem.Title}) was: " + Environment.NewLine + NewCopyItem.ToStringOrDefault();
            }
        }
    }
}
