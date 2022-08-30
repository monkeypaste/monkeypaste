using AvaloniaEdit.Document;
using MonkeyPaste.Common;

using PropertyChanged;
using System.Threading.Tasks;

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

        public override async Task ScrollToSelectedItemAsync() {
            if (SelectedIdx < 0) {
                return;
            }
            MpRect characterRect =  await _matches[SelectedIdx].End.GetCharacterRectAsync(LogicalDirection.Forward);
            
            //AssociatedObject.BringIntoView();
            //AssociatedObject.FileListItemTextBlock.BringIntoView(characterRect);
        }
    }
}
