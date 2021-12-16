using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {

    public class MpFileListItemHighlightBehavior : MpHighlightBehaviorBase<MpFileListItemView> {
        protected override TextRange ContentRange => new TextRange(
            AssociatedObject.FileListItemTextBlock.ContentStart, 
            AssociatedObject.FileListItemTextBlock.ContentEnd);


        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public override void ScrollToSelectedItem() {
            if (SelectedIdx < 0) {
                return;
            }
            Rect characterRect = _matches[SelectedIdx].End.GetCharacterRect(LogicalDirection.Forward);
            
            AssociatedObject.BringIntoView();
            AssociatedObject.FileListItemTextBlock.BringIntoView(characterRect);
        }
    }
}
