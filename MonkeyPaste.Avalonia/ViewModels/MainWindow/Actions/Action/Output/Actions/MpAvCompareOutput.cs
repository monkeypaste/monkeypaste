using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvCompareOutput : MpAvActionOutput {
        public override object OutputData => Matches;
        public List<MpAvConditionalMatch> Matches { get; set; }
        public bool WasConditionMet {
            get {
                bool was_met = Matches != null && Matches.Count > 0;
                if (Flip) {
                    return !was_met;
                }
                return was_met;
            }
        }
        public bool Flip { get; set; }

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
