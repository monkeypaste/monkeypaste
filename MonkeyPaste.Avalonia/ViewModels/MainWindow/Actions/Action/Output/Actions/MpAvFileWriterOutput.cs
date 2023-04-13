namespace MonkeyPaste.Avalonia {
    public class MpAvFileWriterOutput : MpAvActionOutput {
        public string OutputFilesStr { get; set; }
        public override object OutputData => OutputFilesStr;

        public override string ActionDescription {
            get {
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was written to file path: '{OutputFilesStr}'";
            }
        }
    }

}
