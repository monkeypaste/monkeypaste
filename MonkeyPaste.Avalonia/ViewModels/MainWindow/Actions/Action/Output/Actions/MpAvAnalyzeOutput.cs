using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyzeOutput : MpAvActionOutput {
        public override object OutputData => TransactionResult;
        public object TransactionResult { get; set; }
        public override string ActionDescription {
            get {
                return $"Result of analysis of CopyItem({CopyItem.Id},{CopyItem.Title}) was: " + Environment.NewLine + TransactionResult.ToString();
            }
        }
    }
}
