using System.Windows.Documents;

namespace MpWpfApp {
    public class MpClipTileTitleHighlightTBehavior : MpHighlightBehaviorBase<MpClipTileTitleView> {
        protected override TextRange ContentRange => new TextRange(
            AssociatedObject.ClipTileTitleTextBlock.ContentStart,
            AssociatedObject.ClipTileTitleTextBlock.ContentEnd);

        public override MpHighlightType HighlightType => MpHighlightType.Title;
    }

    public class MpRtbHighlightBehavior : MpHighlightBehaviorBase<MpRtbView> {

        protected override TextRange ContentRange => new TextRange(
            AssociatedObject.Rtb.Document.ContentStart, 
            AssociatedObject.Rtb.Document.ContentEnd);

        public override MpHighlightType HighlightType => MpHighlightType.Content;
    }
}
