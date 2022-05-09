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
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Microsoft.Office.Interop.Outlook;

namespace MpWpfApp {
    public class MpContentAdorner : Adorner {
        #region Private Variables
        private bool _isDash = true;

        private Color _debugColor;

        private MpIContentDropTarget _dropBehavior;

        #endregion

        #region Properties

        public bool IsShowingDropShape => DropIdx >= 0;

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
                    ctvm.Items.Count > 1 &&
                    ctvm.IsContentReadOnly &&
                    ctvm.IsAnyHoveringOverItem) {
                    return true;
                }
                return false;
            }
        }

        public bool IsShowing => MpMainWindowViewModel.Instance.IsMainWindowOpen &&
                                 (IsShowingDropShape || 
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
                        new Pen(Brushes.Red, 1.5) { DashStyle = _isDash ? DashStyles.Dash : DashStyles.Solid });
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
            }
        }

        private void DrawShape(DrawingContext dc, MpShape dropShape, Pen pen) {
            if (dropShape is MpLine dl) {
                dc.DrawLine(
                    pen,
                    dl.P1.ToWpfPoint(),
                    dl.P2.ToWpfPoint());
            } else if (dropShape is MpEllipse de) {
                dc.DrawEllipse(
                    Brushes.Transparent,
                    pen,
                    de.Center.ToWpfPoint(),
                    de.Size.Width / 2,
                    de.Size.Height / 2);
            } else if (dropShape is MpRect dr) {
                dc.DrawRectangle(
                    Brushes.Transparent,
                    pen,
                    dr.ToWpfRect());
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
            //int line = 0;
            var lineStartPointer = rtb.Document.ContentStart.GetLineStartPosition(0);
            while (lineStartPointer != null) {
                var lineEndPointer = lineStartPointer.GetLineEndPosition(0);
                Point p1 = lineStartPointer.GetCharacterRect(LogicalDirection.Forward).BottomLeft;
                Point p2 = lineEndPointer.GetCharacterRect(LogicalDirection.Backward).BottomRight;

                dc.DrawLine(
                    pen,
                    p1,
                    p2);

                lineStartPointer = lineEndPointer.GetLineStartPosition(1);

                //MpConsole.WriteLine("Line " + (line++) + " " + p1 + " " + p2);
            }
            //MpConsole.WriteLine("");
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
