namespace MonkeyPaste.Avalonia {
    public interface MpAvITransactionNodeViewModel :
        MpITreeItemViewModel,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpILabelTextViewModel,
        MpISortableViewModel,
        MpIHasIconSourceObjViewModel,
        MpIPlainTextViewModel,
        MpIMenuItemViewModel,
        MpIAsyncCollectionObject {
        string Body { get; }
        MpAvClipTileViewModel HostClipTileViewModel { get; }
    }
}
