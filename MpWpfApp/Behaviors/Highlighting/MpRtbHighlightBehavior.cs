using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {

    public class MpRtbHighlightBehavior : MpHighlightBehaviorBase<MpRtbView> {

        protected override TextRange ContentRange => new TextRange(
            AssociatedObject.Rtb.Document.ContentStart, 
            AssociatedObject.Rtb.Document.ContentEnd);

        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public override void ScrollToSelectedItem() {
            if(SelectedIdx < 0) {
                AssociatedObject.Rtb.ScrollToHome();
                return;
            }

            TextRange tr = _matches[SelectedIdx];
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
