using Avalonia.Controls;
using AvaloniaEdit.Document;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged;
using MonkeyPaste.Common.Utils.Extensions;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvContentHighlightBehavior : MpAvHighlightBehaviorBase<MpAvClipTileContentView> {
        private Dictionary<int, List<MpShape>> _highlightShapes = new Dictionary<int, List<MpShape>>();

        protected override MpAvITextRange ContentRange {
            get {
                if(AssociatedObject.GetVisualDescendant<MpAvCefNetWebView>() is MpAvCefNetWebView wv) {

                    return new MpAvTextRange(
                                wv.Document.ContentStart,
                                wv.Document.ContentEnd);
                }
                return null;
            }
        }

        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public Dictionary<int, List<MpShape>> GetHighlightShapes() {
            FindHighlightShapesAsync();
            return _highlightShapes;
        }

        public override async Task FindHighlightingAsync() {
            await base.FindHighlightingAsync();

            await FindHighlightShapesAsync();
        }

        public async Task FindHighlightShapesAsync() {
            _highlightShapes.Clear();

            for (int i = 0; i < _matches.Count; i++) {
                var match = _matches[i];
                var rects = new List<MpShape>();
                var startRect = await match.Start.GetCharacterRectAsync(LogicalDirection.Forward);
                var endRect = await match.End.GetCharacterRectAsync(LogicalDirection.Backward);
                MpRect rectShape;
                if (startRect.Location.Y == endRect.Location.Y) {
                    //when range is not wrapped make 1 box
                    startRect.Union(endRect);
                    rectShape = new MpRect(startRect.Location, startRect.Size);
                    rects.Add(rectShape);
                } else {
                    var eol_tp = match.Start.GetLineEndPosition(0);
                    var eolRect = await eol_tp.GetCharacterRectAsync(LogicalDirection.Backward);
                    startRect.Union(eolRect);
                    rectShape = new MpRect(startRect.Location, startRect.Size);
                    rects.Add(rectShape);

                    var ctp = eol_tp.GetLineStartPosition(1);
                    while (true) {
                        if (ctp == null || ctp == ctp.DocumentEnd) {
                            break;
                        }
                        var sol_rect = await ctp.GetCharacterRectAsync(LogicalDirection.Forward);


                        if (sol_rect.Location.Y == endRect.Location.Y) {
                            //this line is end of rects
                            sol_rect.Union(endRect);
                            rects.Add(new MpRect(sol_rect.Location, sol_rect.Size));
                            break;
                        }
                        var cur_eol_tp = ctp.GetLineEndPosition(0);
                        eolRect = await cur_eol_tp.GetCharacterRectAsync(LogicalDirection.Backward);
                        sol_rect.Union(eolRect);
                        rects.Add(new MpRect(sol_rect.Location, sol_rect.Size));

                        ctp = ctp.GetLineStartPosition(1);
                    }
                }
                _highlightShapes.Add(i, rects);
            }
        }

        public override async Task ApplyHighlightingAsync() {
            if (AssociatedObject == null) {
                return;
            }
            await ScrollToSelectedItemAsync();

            AssociatedObject.UpdateAdorners();
            AssociatedObject.InvalidateAll();
        }
        public override void ClearHighlighting() {
            base.ClearHighlighting();
            _highlightShapes?.Clear();
        }

        public override async Task ScrollToSelectedItemAsync() {
            var sv = AssociatedObject.GetVisualDescendant<ScrollViewer>();

            if (AssociatedObject == null ||
               _matches.Count == 0) {
                if(sv != null) {
                    sv.ScrollToHome();
                }
                return;
            }
            int idx = Math.Min(Math.Max(0, SelectedIdx), _matches.Count-1);
            var tr = _matches[idx];
            //var iuic = tr.End.Parent.FindParentOfType<InlineUIContainer>();
            //if (iuic != null) {
            //    var rhl = iuic.Parent.FindParentOfType<Hyperlink>();
            //    if (rhl != null) {
            //        tr = new TextRange(rhl.ContentStart, rhl.ContentEnd);
            //    }
            //}

            AssociatedObject.BringIntoView();

            var start = await tr.Start.GetCharacterRectAsync(LogicalDirection.Forward);
            var end = await tr.End.GetCharacterRectAsync(LogicalDirection.Forward);
            double offset = (start.Top + end.Bottom - sv.Viewport.Height) / 2 + sv.Offset.Y;
            if(double.IsNaN(offset) || double.IsInfinity(offset)) {
                offset = 0;
            }
            sv.ScrollToVerticalOffset(offset);
        }

        public void InitLocalHighlighting(List<MpAvITextRange> matches) {
            _matches = matches;
            SelectedIdx = -1;
        }
    }
}
