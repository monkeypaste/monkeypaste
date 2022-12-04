namespace MonkeyPaste.Avalonia {
    public class MpAvClassifyOutput : MpAvActionOutput {
        public override object OutputData => TagId;
        public int TagId { get; set; }
        public override string ActionDescription => $"CopyItem({CopyItem.Id},{CopyItem.Title}) Classified to Tag({TagId})";
    }
}
