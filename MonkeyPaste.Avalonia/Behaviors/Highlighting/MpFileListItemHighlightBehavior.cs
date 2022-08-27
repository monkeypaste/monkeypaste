using AvaloniaEdit.Document;
using MonkeyPaste.Common;

using PropertyChanged;
namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvFileListItemHighlightBehavior : MpAvHighlightBehaviorBase<MpAvClipTileView> { //<MpFileListItemView> {
        protected override MpAvITextRange ContentRange {
            get {
                //if (AssociatedObject == null || AssociatedObject.FileListItemTextBlock == null) {
                //    return null;
                //}
                //return new MpAvTextRange(
                //        AssociatedObject.FileListItemTextBlock.ContentStart,
                //        AssociatedObject.FileListItemTextBlock.ContentEnd);
                return null;
            }
        }


        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public override void ScrollToSelectedItem() {
            if (SelectedIdx < 0) {
                return;
            }
            MpRect characterRect = _matches[SelectedIdx].End.GetCharacterRect(LogicalDirection.Forward);
            
            //AssociatedObject.BringIntoView();
            //AssociatedObject.FileListItemTextBlock.BringIntoView(characterRect);
        }
    }
}
