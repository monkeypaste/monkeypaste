using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpClipTileTitleHighlightBehavior : MpHighlightBehaviorBase<MpClipTileTitleView> {
        protected override TextRange ContentRange {
            get {
                if(AssociatedObject == null || AssociatedObject.ClipTileTitleTextBlock == null) {
                    return null;
                }
                return new TextRange(
                        AssociatedObject.ClipTileTitleTextBlock.ContentStart,
                        AssociatedObject.ClipTileTitleTextBlock.ContentEnd);
            }
        }

        public override MpHighlightType HighlightType => MpHighlightType.Title;

        public override void ScrollToSelectedItem() {
            return;
        }
    }
}
