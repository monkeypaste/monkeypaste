namespace MonkeyPaste {
    public interface MpIPluginPresetTransaction {
        int PresetId { get; }
        MpCopyItemTransactionType TransactionType { get; }
    }
}
