

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileTitleHighlightBehavior : MpAvHighlightBehaviorBase<MpAvClipTileTitleView> {        
        protected override MpAvITextRange ContentRange {
            get {
                return null;
                //if(AssociatedObject == null || AssociatedObject.ClipTileTitleTextBox == null) {
                //    return null;
                //}
                //var tb = new TextBlock() {
                //    Text = AssociatedObject.ClipTileTitleTextBox.Text
                //};
                //return new TextRange(tb.ContentStart,tb.ContentEnd);
            }
        }

        public override MpHighlightType HighlightType => MpHighlightType.Title;

        public override void ScrollToSelectedItem() {
            return;
        }
    }
}
