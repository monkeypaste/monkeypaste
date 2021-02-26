using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpRichTextBoxOverlayAdorner : Adorner {
        #region Private Variables
        private Canvas _rtbc;
        private RichTextBox _rtb;
        private ListBox _rtblb;
        #endregion

        #region Public Methods
        public MpRichTextBoxOverlayAdorner(Canvas rtbc) : base(rtbc) {
            _rtbc = rtbc;
            _rtb = (RichTextBox)_rtbc.FindName("ClipTileRichTextBox");
            _rtblb = rtbc.GetVisualAncestor<ListBox>();
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            var rtbvm = (MpClipTileRichTextBoxViewModel)_rtbc.DataContext;  
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var blackPen = new Pen(Brushes.Black, 2);
            var redPen = new Pen(Brushes.Red, 2);
            if (!rtbvm.ClipTileViewModel.IsSelected) {
                return;
            }

            //if(rtbvm.Previous != null) {
            //    drawingContext.DrawLine(blackPen, adornedElementRect.TopLeft, adornedElementRect.TopRight);
            //}
            //if (rtbvm.Next != null) {
            //    drawingContext.DrawLine(blackPen, adornedElementRect.BottomLeft, adornedElementRect.BottomRight);
            //}

            if(rtbvm.IsDragging) {
                var mpos = MpHelpers.Instance.GetMousePosition(_rtblb);
                var hitTest = VisualTreeHelper.HitTest(_rtblb, mpos);
                if(hitTest != null && hitTest.VisualHit != null) {
                    var hitResult = hitTest.VisualHit;
                    MpClipTileRichTextBoxViewModel hrtbvm = null;
                    if(hitResult is Path) {
                        hrtbvm = ((MpRichTextBoxPathOverlayViewModel)(hitResult as Path).DataContext).ClipTileRichTextBoxViewModel;
                    } else if(hitResult is Canvas || hitResult is RichTextBox) {
                        hrtbvm = ((hitResult as FrameworkElement).DataContext as MpClipTileRichTextBoxViewModel);
                    } else if(hitResult is ListBox) {
                        if(mpos.Y < 5) {
                            hrtbvm = rtbvm.RichTextBoxViewModelCollection[0];
                        } else {
                            hrtbvm = rtbvm.RichTextBoxViewModelCollection[rtbvm.RichTextBoxViewModelCollection.Count-1];
                        }
                    } else if(hitResult is MpClipBorder) {
                        var ctvm = (MpClipTileViewModel)(hitResult as MpClipBorder).DataContext;
                        mpos = MpHelpers.Instance.GetMousePosition(ctvm.RichTextBoxListBox);
                        int targetRtbIdx = -1;
                        if(mpos.Y < 0) {
                            targetRtbIdx = 0;
                        } else if(mpos.Y > ctvm.RichTextBoxListBox.ActualHeight) {
                            targetRtbIdx = ctvm.RichTextBoxViewModelCollection.Count - 1;
                        } else {
                            for (int i = 0; i < ctvm.RichTextBoxViewModelCollection.Count; i++) {
                                var ctrtbvm = ctvm.RichTextBoxViewModelCollection[i];
                                if (mpos.Y <= ctrtbvm.RtbListBoxItemHeight) {
                                    targetRtbIdx = i;
                                }
                            }
                        }
                        if(targetRtbIdx < 0) {
                            return;
                        }
                        hrtbvm = ctvm.RichTextBoxViewModelCollection[targetRtbIdx];
                    }
                    if(hrtbvm == null/* || hrtbvm == rtbvm*/) {
                        return;
                    }
                    var hmpos = _rtblb.TranslatePoint(mpos, hrtbvm.Rtbc);
                    var hitRect = new Rect(0, 0, hrtbvm.Rtbc.ActualWidth, hrtbvm.Rtbc.ActualHeight);

                    if(Math.Abs(hmpos.X - hitRect.Left) < 10) {
                        //adorn left side
                    } else if (Math.Abs(hmpos.X - hitRect.Right) < 10) {
                        //adorn right side
                    } else if (Math.Abs(hmpos.Y - hitRect.Top) < hitRect.Height / 2) {
                        //adorn top
                        var p0 = hrtbvm.Rtbc.TranslatePoint(new Point(0, 0), rtbvm.Rtbc);
                        var p1 = hrtbvm.Rtbc.TranslatePoint(new Point(hitRect.Right, 0), rtbvm.Rtbc);
                        drawingContext.DrawLine(blackPen, p0, p1);
                    } else {
                        //adorn bottom
                    }
                }
            }
        }
        #endregion
    }
}
