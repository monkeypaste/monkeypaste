using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvActiveAppChangedTriggerOutput : MpAvActionOutput {
        public override object OutputData => ProcessInfo;

        public MpPortableProcessInfo ProcessInfo { get; set; }
        public override string ActionDescription => $"{ProcessInfo} Activated";
    }
}
