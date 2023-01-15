namespace MonkeyPaste {
    public interface MpIPluginPresetTransaction {
        int PresetId { get; }
        MpTransactionSourceType TransactionType { get; }
    }
}
