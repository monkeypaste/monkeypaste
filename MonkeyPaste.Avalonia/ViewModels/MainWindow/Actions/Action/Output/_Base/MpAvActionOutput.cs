namespace MonkeyPaste.Avalonia {
    public abstract class MpAvActionOutput {
        public MpCopyItem CopyItem { get; set; }
        public MpAvActionOutput Previous { get; set; }
        public abstract object OutputData { get; }
        public abstract string ActionDescription { get; }
    }
}
