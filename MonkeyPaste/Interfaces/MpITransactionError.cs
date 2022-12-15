namespace MonkeyPaste {
    //public interface MpISourceItem {
    //    //MpIcon SourceIcon { get; }
    //    int IconId { get; }
    //    string SourcePath { get; }
    //    string SourceName { get; }

    //    int RootId { get; }

    //    bool IsUrl { get; }
    //    bool IsDll { get; }
    //    bool IsExe { get; }        
    //    bool IsUser { get; }

    //    // TODO I think using MpSourceItemMedium is better than above bool's
    //    //MpSourceItemMedium SourceMedium { get; }

    //    bool IsRejected { get; }
    //    bool IsSubRejected { get; }
    //}

    //public interface MpISourceItemViewModel : MpISourceItem, MpIViewModel { }

    public interface MpITransactionError {
        string TransactionErrorMessage { get; set; }
    }
}
