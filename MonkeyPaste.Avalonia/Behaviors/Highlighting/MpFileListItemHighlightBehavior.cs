
using MonkeyPaste.Common;

using PropertyChanged;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvFileListItemHighlightBehavior : MpAvHighlightBehaviorBase<MpAvClipTileView> { //<MpFileListItemView> {
        protected override MpTextRange ContentRange {
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
        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
        }

        public override void ClearHighlighting() {
            base.ClearHighlighting();
        }

        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);
        }
    }
}
