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
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using MonkeyPaste.Common.Wpf;
using Microsoft.Office.Interop.Outlook;
using System.Diagnostics;

namespace MpWpfApp {
    public class MpContentAdorner : Adorner {
        #region Private Variables
        private bool _isDash = true;

        private Color _debugColor;

        private MpIContentDropTarget _dropBehavior;
        private MpRtbHighlightBehavior _highlightBehavior;

        #endregion

        #region Statics
        #endregion

        #region Properties

        public Brush DropShapeBrush {
            get {
                if(_dropBehavior == null) {
                    return Brushes.Transparent;
                }
                if(_dropBehavior is MpPinTrayDropBehavior) {
                    return Brushes.OldLace;
                }
                if (_dropBehavior is MpActionDesignerItemDropBehavior) {
                    return Brushes.LightBlue;
                }
                return Brushes.Red;
            }
        }
        public bool IsShowingDropShape => DropIdx >= 0;

        public bool IsShowingHighlightShapes => _highlightBehavior != null && _highlightBehavior.GetHighlightShapes().Count > 0;

        public bool IsShowingCaret {
            get {
                //if(AdornedElement != null && 
                //   AdornedElement is RichTextBox rtb &&
                //   rtb.Selection.IsEmpty) {// &&
                //   //rtb.DataContext is MpClipTileViewModel ctvm &&
                //   //ctvm.IsSubSelectionEnabled) {
                //    return true;
                //}
                return false;
            }
        }

        public bool IsShowingContentLines {
            get {
                if (AdornedElement != null &&
                   AdornedElement is RichTextBox rtb &&
                    rtb.DataContext is MpClipTileViewModel ctvm &&
                    ctvm.IsSubSelectionEnabled &&
                    ctvm.IsContentReadOnly) {
                    return true;
                }
                return false;
            }
        }

        public bool IsShowingContentBounds {
            get {
                if (AdornedElement != null &&
                   AdornedElement is RichTextBox rtb &&
                    rtb.DataContext is MpClipTileViewModel ctvm &&
                    ctvm.IsContentReadOnly &&
                    ctvm.IsHovering) {
                    return true;
                }
                return false;
            }
        }

        public bool IsShowing => //MpMainWindowViewModel.Instance.IsMainWindowOpen &&
                                (_dropBehavior.GetType() != typeof(MpPinTrayDropBehavior) ||
                                 IsShowingHighlightShapes) &&
                                 (IsShowingDropShape || 
                                  IsShowingHighlightShapes ||
                                 IsShowingCaret || 
                                 IsDebugMode || 
                                 IsShowingContentLines);

        public bool IsDebugMode {
            get {
                if(_dropBehavior == null) {
                    return false;
                }
                return _dropBehavior.IsDebugEnabled;
            }
        }

        public List<Rect> DropRects { 
            get {
                if(_dropBehavior == null) {
                    return new List<Rect>();
                }
                return _dropBehavior.DropRects;
            }
        } 

        public int DropIdx {
            get {
                if (_dropBehavior == null) {
                    return -1;
                }
                return _dropBehavior.DropIdx;
            }
        }

        


        #endregion

        #region Public Methods
        public MpContentAdorner(UIElement uie, MpIContentDropTarget dropBehavior) : base(uie) {
            _dropBehavior = dropBehavior;

            var rtbcv = uie.GetVisualAncestor<MpRtbContentView>();
            if(rtbcv != null) {                
                _highlightBehavior = rtbcv.RtbHighlightBehavior;
            }
            
            
            _isDash = _dropBehavior.GetType() != typeof(MpActionDesignerItemDropBehavior);

            _debugColor = MpWpfColorHelpers.GetRandomColor();
            _debugColor.A = 50;
            
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext dc) {
            if (IsShowing) {
                Visibility = Visibility.Visible;

                if (IsDebugMode) {
                    DrawDebugRects(dc);
                }

                if (IsShowingDropShape) {
                    DrawDropShapes(
                        dc,
                        new Pen(DropShapeBrush, 1.5) { DashStyle = _isDash ? DashStyles.Dash : DashStyles.Solid });
                }

                if (IsShowingHighlightShapes) {
                    DrawHighlightShapes(dc);
                }
                if (IsShowingCaret) {
                    DrawCaret(dc, new Pen(Brushes.Red, 1));
                }


                if (IsShowingContentLines) {
                    DrawUnderlines(dc, new Pen(Brushes.DimGray, 1));
                } 

                if(IsShowingContentBounds) {

                }

                
            } else {
                Visibility = Visibility.Hidden;
                Opacity = 1;
            }
        }

        private void DrawShape(DrawingContext dc, MpShape dropShape, Pen pen, Brush brush = null) {
            var fe = AdornedElement as FrameworkElement;
            if (fe == null) {
                return;
            }
            brush = brush == null ? Brushes.Transparent : brush;

            var rtb_rect = fe.Bounds();
            if (dropShape is MpLine dl) {
                if (!rtb_rect.Contains(dl.P1.ToWpfPoint()) || !rtb_rect.Contains(dl.P2.ToWpfPoint())) {
                    return;
                }

                dc.DrawLine(
                    pen,
                    dl.P1.ToWpfPoint(),
                    dl.P2.ToWpfPoint());
            } else if (dropShape is MpEllipse de) {
                dc.DrawEllipse(
                    brush,
                    pen,
                    de.Center.ToWpfPoint(),
                    de.Size.Width / 2,
                    de.Size.Height / 2);
            } else if (dropShape is MpRect dr) {
                dc.DrawRectangle(
                    brush,
                    pen,
                    dr.ToWpfRect());
            }
        }

        private void DrawHighlightShapes(DrawingContext dc) {
            var rtb = AdornedElement as RichTextBox;
            if (rtb == null) {
                return;
            }
            this.Opacity = 0.5;
            var sv = rtb.GetVisualDescendent<ScrollViewer>();

            var dropShapes = _highlightBehavior.GetHighlightShapes();

            foreach(var kvp in dropShapes) {
                foreach(MpRect rect in kvp.Value) {
                    //rect.Location = rtb.TranslatePoint(rect.Location.ToWpfPoint(), sv).ToMpPoint();
                    DrawShape(
                        dc,
                        rect,
                        new Pen(),
                        kvp.Key == _highlightBehavior.SelectedIdx ? //Brushes.Crimson : Brushes.Pink);
                            _highlightBehavior.ActiveHighlightBrush :
                            _highlightBehavior.InactiveHighlightBrush);
                }
            }
        }

        private void DrawDropShapes(DrawingContext dc, Pen pen) {
            var dropShapes = _dropBehavior.GetDropTargetAdornerShape();
            dropShapes.ForEach(x => DrawShape(dc, x, pen));
        }

        private void DrawUnderlines(DrawingContext dc, Pen pen) {
            var rtb = AdornedElement as RichTextBox;
            if(rtb == null) {
                return;
            }
            var sv = rtb.GetVisualDescendent<ScrollViewer>();
            var lineStartPointer = rtb.Document.ContentStart.GetLineStartPosition(0);
            while (lineStartPointer != null && lineStartPointer != rtb.Document.ContentEnd) {
                var lineEndPointer = lineStartPointer.GetLineEndPosition(0);
                Point p1 = lineStartPointer.GetCharacterRect(LogicalDirection.Forward).BottomLeft;
                Point p2 = lineEndPointer.GetCharacterRect(LogicalDirection.Backward).BottomRight;

                // this ensures the line doesn't jump when run is wrapped
                p2.Y = p1.Y;
                lineStartPointer = lineEndPointer.GetLineStartPosition(1);
                DrawShape(dc, new MpLine(p1.ToMpPoint(), p2.ToMpPoint()), pen);
            }
        }

        private void DrawCaret(DrawingContext dc, Pen pen) {
            var rtb = AdornedElement as RichTextBox;
            if (rtb == null) {
                return;
            }
            if (!rtb.Selection.IsEmpty) {
                return;
            }

            var caret_rect = rtb.CaretPosition.GetCharacterRect(LogicalDirection.Forward);

            dc.DrawLine(
                pen,
                caret_rect.TopLeft,
                caret_rect.BottomLeft);
        }

        private void DrawContentHull(DrawingContext dc, Pen pen) {
            //var rtb = AdornedElement as RichTextBox;
            //if (rtb == null) {
            //    return;
            //}
            //var ctvm = rtb.DataContext as MpClipTileViewModel;
            //var fd = rtb.Document;
            //var allRuns = fd.GetAllTextElements().Where(x => x is Run);

            //allRuns.ForEach(x =>
            //      new TextRange(x.ContentStart, x.ContentEnd)
            //      .ApplyPropertyValue(
            //          TextElement.BackgroundProperty, 
            //          x.Tag != null && ctvm.HoverItem != null && (x.Tag as MpCopyItem).Id == ctvm.HoverItem.CopyItemId ? 
            //                Brushes.Yellow :
            //                Brushes.Transparent));
        }

        private void DrawDebugRects(DrawingContext dc) {
            foreach (var debugRect in DropRects) {
                if (DropRects.IndexOf(debugRect) == DropIdx) {
                    dc.DrawRectangle(
                        Brushes.Blue,
                        new Pen(Brushes.Orange, 1),
                        debugRect);
                } else {
                    dc.DrawRectangle(
                        new SolidColorBrush(_debugColor),
                        new Pen(Brushes.Orange, 1),
                        debugRect);
                }
            }
        }
        #endregion
    }
}
