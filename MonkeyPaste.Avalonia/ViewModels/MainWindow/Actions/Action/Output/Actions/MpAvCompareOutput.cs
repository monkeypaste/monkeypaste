using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvCompareOutput : MpAvActionOutput {
        public override object OutputData => Matches;
        public List<MpAvConditionalMatch> Matches { get; set; }
        public bool WasConditionMet =>
            Matches != null && Matches.Count > 0;

        public override string ActionDescription {
            get {
                if (Matches == null || Matches.Count == 0) {
                    return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was NOT a match";
                }
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was matched w/ Match Value: {string.Join(Environment.NewLine, Matches)}";
            }
        }
    }
}
