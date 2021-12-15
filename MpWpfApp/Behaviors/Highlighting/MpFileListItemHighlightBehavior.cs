using System.Windows.Documents;

namespace MpWpfApp {
    public class MpFileListItemHighlightBehavior : MpHighlightBehaviorBase<MpFileListItemView> {
        protected override TextRange ContentRange => new TextRange(
            AssociatedObject.FileListItemTextBlock.ContentStart, 
            AssociatedObject.FileListItemTextBlock.ContentEnd);


        public override MpHighlightType HighlightType => MpHighlightType.Content;
    }
}
