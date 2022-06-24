using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {

    public class MpRtbHighlightBehavior : MpHighlightBehaviorBase<MpRtbContentView> {
        private Dictionary<int, List<MpShape>> _highlightShapes = new Dictionary<int, List<MpShape>>();

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

        public Dictionary<int, List<MpShape>> GetHighlightShapes() {
            FindHighlightShapes();
            return _highlightShapes;
        }

        public override async Task FindHighlighting() {
            await base.FindHighlighting();

            FindHighlightShapes();
        }

        public void FindHighlightShapes() {
            _highlightShapes.Clear();

            for (int i = 0; i < _matches.Count; i++) {
                var match = _matches[i];
                var rects = new List<MpShape>();
                var startRect = match.Start.GetCharacterRect(LogicalDirection.Forward);
                var endRect = match.End.GetCharacterRect(LogicalDirection.Backward);
                MpRect rectShape;
                if (startRect.Location.Y == endRect.Location.Y) {
                    //when range is not wrapped make 1 box
                    startRect.Union(endRect);
                    rectShape = new MpRect(startRect.Location.ToPortablePoint(), startRect.Size.ToPortableSize());
                    rects.Add(rectShape);
                } else {
                    var eol_tp = match.Start.GetLineEndPosition(0);
                    var eolRect = eol_tp.GetCharacterRect(LogicalDirection.Backward);
                    startRect.Union(eolRect);
                    rectShape = new MpRect(startRect.Location.ToPortablePoint(), startRect.Size.ToPortableSize());
                    rects.Add(rectShape);

                    var ctp = eol_tp.GetLineStartPosition(1);
                    while (true) {
                        if (ctp == null || ctp == ctp.DocumentEnd) {
                            break;
                        }
                        var sol_rect = ctp.GetCharacterRect(LogicalDirection.Forward);


                        if (sol_rect.Location.Y == endRect.Location.Y) {
                            //this line is end of rects
                            sol_rect.Union(endRect);
                            rects.Add(new MpRect(sol_rect.Location.ToPortablePoint(), sol_rect.Size.ToPortableSize()));
                            break;
                        }
                        eolRect = ctp.GetLineEndPosition(0).GetCharacterRect(LogicalDirection.Backward);
                        sol_rect.Union(eolRect);
                        rects.Add(new MpRect(sol_rect.Location.ToPortablePoint(), sol_rect.Size.ToPortableSize()));

                        ctp = ctp.GetLineStartPosition(1);
                    }
                }
                _highlightShapes.Add(i, rects);
            }
        }

        public override void ApplyHighlighting() {
            if (AssociatedObject == null) {
                return;
            }
            ScrollToSelectedItem();

            AssociatedObject.UpdateAdorners();
            AssociatedObject.UpdateLayout();
        }
        public override void ClearHighlighting() {
            base.ClearHighlighting();
            _highlightShapes?.Clear();
        }

        public override void ScrollToSelectedItem() {
            if(AssociatedObject == null ||
               _matches.Count == 0) {
                AssociatedObject.Rtb.ScrollToHome();
                return;
            }
            int idx = Math.Min(Math.Max(0, SelectedIdx), _matches.Count-1);
            TextRange tr = _matches[idx];
            var iuic = tr.End.Parent.FindParentOfType<InlineUIContainer>();
            if (iuic != null) {
                var rhl = iuic.Parent.FindParentOfType<Hyperlink>();
                if (rhl != null) {
                    tr = new TextRange(rhl.ContentStart, rhl.ContentEnd);
                }
            }

            AssociatedObject.BringIntoView();

            var sv = AssociatedObject.Rtb.GetVisualDescendent<ScrollViewer>();
            var start = tr.Start.GetCharacterRect(LogicalDirection.Forward);
            var end = tr.End.GetCharacterRect(LogicalDirection.Forward);
            double offset = (start.Top + end.Bottom - sv.ViewportHeight) / 2 + sv.VerticalOffset;
            if(double.IsNaN(offset) || double.IsInfinity(offset)) {
                offset = 0;
            }
            sv.ScrollToVerticalOffset(offset);
        }

        public void InitLocalHighlighting(List<TextRange> matches) {
            _matches = matches;
            SelectedIdx = -1;
        }
    }
}
