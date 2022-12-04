namespace MonkeyPaste.Avalonia {
    public class MpAvFileWriterOutput : MpAvActionOutput {        
        public string OutputFilePath { get; set; }
        public override object OutputData => OutputFilePath;

        public override string ActionDescription {
            get {
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was written to file path: '{OutputFilePath}'";
            }
        }
    }
}
