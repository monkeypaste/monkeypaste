using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardChangedTriggerOutput : MpAvActionOutput {
        public override object OutputData => ClipboardDataObject;

        public MpPortableDataObject ClipboardDataObject { get; set; }
        public override string ActionDescription => $"Clipboard Changed to: {ClipboardDataObject}";
    }
}
