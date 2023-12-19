using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvActionOutput : MpIActionOutputNode {
        public MpAvActionOutput Previous { get; set; }
        public abstract object OutputData { get; }
        public abstract string ActionDescription { get; }

        MpIActionOutputNode MpIActionOutputNode.Previous => Previous;

        object MpIActionOutputNode.Output => OutputData;
        string MpILabelText.LabelText => ActionDescription;

        public MpCopyItem CopyItem { get; set; }
        public override string ToString() {
            return OutputData.ToStringOrDefault();
        }
    }

}
