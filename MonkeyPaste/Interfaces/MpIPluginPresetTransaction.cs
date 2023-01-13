namespace MonkeyPaste {
    public interface MpIPluginPresetTransaction {
        int PresetId { get; }
        MpCopyItemSourceType TransactionType { get; }
    }
}
