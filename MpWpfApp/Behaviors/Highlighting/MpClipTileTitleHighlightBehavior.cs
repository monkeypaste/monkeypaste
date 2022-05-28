using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpClipTileTitleHighlightBehavior : MpHighlightBehaviorBase<MpClipTileTitleView> {        
        protected override TextRange ContentRange {
            get {
                if(AssociatedObject == null || AssociatedObject.ClipTileTitleTextBox == null) {
                    return null;
                }
                var tb = new TextBlock() {
                    Text = AssociatedObject.ClipTileTitleTextBox.Text
                };
                return new TextRange(tb.ContentStart,tb.ContentEnd);
            }
        }

        public override MpHighlightType HighlightType => MpHighlightType.Title;

        public override void ScrollToSelectedItem() {
            return;
        }
    }
}
