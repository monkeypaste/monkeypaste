namespace MonkeyPaste.Avalonia {
    public class MpAvTriggerInput : MpAvActionOutput {
        public override object OutputData => CopyItem;
        public override string ActionDescription => "Trigger Activated...";
    }
}
