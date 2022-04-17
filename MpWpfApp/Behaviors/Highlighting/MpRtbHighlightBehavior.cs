using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {

    public class MpRtbHighlightBehavior : MpHighlightBehaviorBase<MpContentView> {
        protected override TextRange ContentRange {
            get {
                if(AssociatedObject == null || 
                   AssociatedObject.Rtb == null || 
                   AssociatedObject.Rtb.Document == null) {
                    return null;
                }
                return new TextRange(
                            AssociatedObject.Rtb.Document.ContentStart,
                            AssociatedObject.Rtb.Document.ContentEnd);
            }
        }

        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public override void ScrollToSelectedItem() {
            if(AssociatedObject == null ||
               _matches.Count == 0) {
                AssociatedObject.Rtb.ScrollToHome();
                return;
            }
            int idx = Math.Max(0, SelectedIdx);
            TextRange tr = _matches[idx];
            var iuic = tr.End.Parent.FindParentOfType<InlineUIContainer>();
            if (iuic != null) {
                var rhl = iuic.Parent.FindParentOfType<Hyperlink>();
                if (rhl != null) {
                    //characterRect = rhl.ContentEnd.GetCharacterRect(LogicalDirection.Backward);
                    tr = new TextRange(rhl.ContentStart, rhl.ContentEnd);
                }
            }

            AssociatedObject.BringIntoView();

            var sv = AssociatedObject.Rtb.GetVisualDescendent<ScrollViewer>();
            var start = tr.Start.GetCharacterRect(LogicalDirection.Forward);
            var end = tr.End.GetCharacterRect(LogicalDirection.Forward);
            sv.ScrollToVerticalOffset((start.Top + end.Bottom - sv.ViewportHeight) / 2 + sv.VerticalOffset);


            //Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate { }));

            //AssociatedObject.Rtb.BringIntoView(characterRect);
            //AssociatedObject.Rtb.GetVisualDescendent<ScrollViewer>().ScrollToVerticalOffset(characterRect.Top);
        }
    }
}
