namespace MonkeyPaste.Avalonia {
    public class MpAvMacroOutput : MpAvActionOutput {
        public override object OutputData => CommandPresetGuid;

        public string CommandPresetGuid { get; set; }
        public override string ActionDescription {
            get {
                if (string.IsNullOrEmpty(CommandPresetGuid)) {
                    return $"CopyItem({CopyItem.Id},{CopyItem.Title}) did not have criteria for a macro";
                }
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was embedded with Analyzer {CommandPresetGuid} ";
            }
        }
    }
}
