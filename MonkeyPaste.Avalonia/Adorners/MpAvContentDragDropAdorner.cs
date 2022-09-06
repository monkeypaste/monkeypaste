using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;

using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia;
using System.Threading;
using Avalonia.Rendering;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentDragDropAdorner : MpAvAdornerBase {
        #region Private Variables
        private bool _isDash = true;

        private Color _debugColor;

        private MpAvIContentDropTargetAsync _dropBehavior;

        private MpShape[] _adornerShapes = new MpShape[] { };

        private List<MpRect> _debug_dropRects = new List<MpRect>();

        public int _dropIdx { get; private set; } = -1;

        //private MpRtbHighlightBehavior _highlightBehavior;

        #endregion

        #region Statics
        #endregion

        #region Properties

        public IBrush DropShapeBrush {
            get {
                if (_dropBehavior == null) {
                    return Brushes.Transparent;
                }
                if (_dropBehavior is MpAvPinTrayDropBehavior) {
                    return Brushes.OldLace;
                }
                if (_dropBehavior is MpAvActionDesignerItemDropBehavior) {
                    return Brushes.LightBlue;
                }
                return Brushes.Red;
            }
        }

        
        public bool IsShowingDropShape => _dropIdx >= 0;

        public bool IsShowingHighlightShapes => false;// _highlightBehavior != null && _highlightBehavior.GetHighlightShapes().Count > 0;

        //public bool IsShowingCaret {
        //    get {
        //        //if (AdornedControl != null &&
        //        //   AdornedControl is MpAvIContentView rtb &&
        //        //   rtb.Selection.IsEmptyAsync) {// &&
        //        //                           //rtb.DataContext is MpClipTileViewModel ctvm &&
        //        //                           //ctvm.IsSubSelectionEnabled) {
        //        //    return true;
        //        //}
        //        return false;
        //    }
        //}

        public bool IsShowingContentLines {
            get {
                if (AdornedControl != null &&
                    AdornedControl.DataContext is MpAvClipTileViewModel ctvm &&
                    ctvm.IsSubSelectionEnabled &&
                    ctvm.IsContentReadOnly) {
                    return true;
                }
                return false;
            }
        }

        public bool IsShowingContentBounds {
            get {
                if (AdornedControl != null &&
                    AdornedControl.DataContext is MpAvClipTileViewModel ctvm &&
                    ctvm.IsContentReadOnly &&
                    ctvm.IsHovering) {
                    return true;
                }
                return false;
            }
        }

        //public bool IsShowing => //MpMainWindowViewModel.Instance.IsMainWindowOpen &&
        //                        (_dropBehavior.GetType() != typeof(MpAvPinTrayDropBehavior) ||
        //                         IsShowingHighlightShapes) &&
        //                         (IsShowingDropShape ||
        //                          IsShowingHighlightShapes ||
        //                         IsShowingCaret ||
        //                         IsDebugMode ||
        //                         IsShowingContentLines);

        public bool IsShowing {
            get {
                if(IsDebugMode) {
                    return true;
                }
                if(IsShowingDropShape) {
                    return true;
                }
                if(IsShowingHighlightShapes) {
                    return true;
                }
                return false;
            }
        }

        public bool IsDebugMode {
            get {
                if (_dropBehavior == null) {
                    return false;
                }
                return _dropBehavior.IsDebugEnabled;
            }
        }

        //public List<MpRect> DropRects {
        //    get {
        //        if (_dropBehavior == null) {
        //            return new List<MpRect>();
        //        }
        //        return _dropBehavior.DropRects;
        //    }
        //}

        //public List<MpRect> DropRects { get; set; } = new List<MpRect>();


        #endregion
        DispatcherTimer _timer;
        #region Public Methods
        public MpAvContentDragDropAdorner(Control uie, MpAvIContentDropTargetAsync dropBehavior) : base(uie) {
            _dropBehavior = dropBehavior;

            var rtbcv = uie.GetVisualAncestor<MpAvClipTileContentView>();
            if (rtbcv != null) {
                //_highlightBehavior = rtbcv.RtbHighlightBehavior;
            }

            IsVisible = true;
            _isDash = _dropBehavior.GetType() != typeof(MpAvActionDesignerItemDropBehavior);

            _debugColor = MpColorHelpers.GetRandomHexColor().AdjustAlpha(0.25).ToAvColor();
            
        }

        public void StartRenderTimer() {
            if(_timer == null) {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(100);
                _timer.Tick += _timer_Tick;
            }
            _timer.Start();
        }
        public void StopRenderTimer() {
            if(_timer == null) {
                return;
            }
            _timer.Stop();
        }

        private async void _timer_Tick(object sender, EventArgs e) {
            _debug_dropRects = await _dropBehavior.GetDropTargetRectsAsync();
            _adornerShapes = await _dropBehavior.GetDropTargetAdornerShapeAsync();

            if(MpAvDragDropManager.CurDropTarget == _dropBehavior) {
                _dropIdx = MpAvDragDropManager.CurDropTarget.DropIdx;
            } else {
                _dropIdx = -1;
            }

            this.InvalidateVisual();

        }
        #endregion
        #region Overrides
        public override void Render(DrawingContext dc) {
           // 
            if (IsShowing) {
                IsVisible = true;

                if (IsDebugMode) {
                    //DrawDebugRects(dc);
                    //dc.DrawLine(new Pen(Brushes.White, 5), new Point(50, 0), new Point(50, 500));
                    _debug_dropRects.ForEach(x => DrawShape(dc, x));
                }

                if (IsShowingDropShape) {
                    //DrawAdornerShapes(
                    //    dc,
                    //    new Pen(DropShapeBrush, 1.5) { DashStyle = _isDash ? DashStyle.Dash : new DashStyle() });
                    _adornerShapes.ForEach(x => DrawShape(dc, x));
                }

                if (IsShowingHighlightShapes) {
                    DrawHighlightShapes(dc);
                }
                //if (IsShowingCaret) {
                //    DrawCaret(dc, new Pen(Brushes.Red, 1));
                //}


                if (IsShowingContentLines) {
                    DrawUnderlines(dc, new Pen(Brushes.DimGray, 1));
                }

                if (IsShowingContentBounds) {

                }


            } else {
                IsVisible = false;
                //Opacity = 1;
            }
            base.Render(dc);
        }

        private void DrawShape(DrawingContext dc, MpShape shape) {
            var fe = AdornedControl as Control;
            if (fe == null) {
                return;
            }
            IBrush brush = shape.FillOctColor.ToAvBrush();


            IPen pen = null;// shape.StrokeThickness.ToA;
            brush = brush == null ? Brushes.Transparent : brush;

            if (shape is MpLine dl) {
                dc.DrawLine(
                    pen,
                    dl.P1.ToAvPoint(),
                    dl.P2.ToAvPoint());
            } else if (shape is MpEllipse de) {
                dc.DrawEllipse(
                    brush,
                    pen,
                    de.Center.ToAvPoint(),
                    de.Size.Width / 2,
                    de.Size.Height / 2);
            } else if (shape is MpRect dr) {
                dc.DrawRectangle(
                    brush,
                    pen,
                    dr.ToAvRect());
            }
        }

        private void DrawHighlightShapes(DrawingContext dc) {
            var rtb = AdornedControl;
            if (rtb == null) {
                return;
            }
            this.Opacity = 0.5;
            var sv = rtb.GetVisualDescendant<ScrollViewer>();

            //var dropShapes = _highlightBehavior.GetHighlightShapes();

            //foreach (var kvp in dropShapes) {
            //    foreach (MpRect rect in kvp.Value) {
            //        if (rect.Points.Any(x => !x.X.IsNumber() || !x.Y.IsNumber())) {
            //            // this seems to occur when flow doc has templates
            //            continue;
            //        }
            //        //rect.Location = rtb.TranslatePoint(rect.Location.ToAvPoint(), sv).ToMpPoint();
            //        DrawShape(
            //            dc,
            //            rect,
            //            new Pen(),
            //            kvp.Key == _highlightBehavior.SelectedIdx ? //Brushes.Crimson : Brushes.Pink);
            //                _highlightBehavior.ActiveHighlightBrush :
            //                _highlightBehavior.InactiveHighlightBrush);
            //    }
            //}
        }

        //private void DrawAdornerShapes(DrawingContext dc, Pen pen) {
        //    //Dispatcher.UIThread.Post(async () => {
        //    //    var dropShapes = await _dropBehavior.GetDropTargetAdornerShapeAsync();
        //    //    DropRects.ForEach(x => DrawShape(dc, x, pen));
        //    //});
        //    _adornerShapes.ForEach(x => DrawShape(dc, x, pen));
        //}

        private void DrawUnderlines(DrawingContext dc, Pen pen) {
            //var rtb = AdornedControl as RichTextBox;
            //if (rtb == null) {
            //    return;
            //}
            //var sv = rtb.GetVisualDescendent<ScrollViewer>();
            //var lineStartPointer = rtb.Document.ContentStart.GetLineStartPosition(0);
            //while (lineStartPointer != null && lineStartPointer != rtb.Document.ContentEnd) {
            //    var lineEndPointer = lineStartPointer.GetLineEndPosition(0);
            //    Point p1 = lineStartPointer.GetCharacterRect(LogicalDirection.Forward).BottomLeft;
            //    Point p2 = lineEndPointer.GetCharacterRect(LogicalDirection.Backward).BottomRight;

            //    // this ensures the line doesn't jump when run is wrapped
            //    p2.Y = p1.Y;
            //    lineStartPointer = lineEndPointer.GetLineStartPosition(1);
            //    DrawShape(dc, new MpLine(p1.ToPortablePoint(), p2.ToPortablePoint()), pen);
            //}
        }

        private void DrawCaret(DrawingContext dc, Pen pen) {
            //var rtb = AdornedControl as RichTextBox;
            //if (rtb == null) {
            //    return;
            //}
            //if (!rtb.Selection.IsEmpty) {
            //    return;
            //}

            //var caret_rect = rtb.CaretPosition.GetCharacterRect(LogicalDirection.Forward);

            //dc.DrawLine(
            //    pen,
            //    caret_rect.TopLeft,
            //    caret_rect.BottomLeft);
        }

        private void DrawContentHull(DrawingContext dc, Pen pen) {
            //var rtb = AdornedControl as RichTextBox;
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
            //if(sc == null) {
            //    return;
            //}dc.PlatformImpl
            //sc.Post(async _ => {
            //    var dtrl = await _dropBehavior.GetDropTargetRectsAsync();
            //    foreach (var debugRect in dtrl) {
            //        bool isDropIdx = dtrl.IndexOf(debugRect) == _dropIdx;

            //        var av_rect = debugRect.ToAvRect();
            //        IBrush brush = isDropIdx ? Brushes.Blue : new SolidColorBrush(_debugColor);
            //        IPen pen = new Pen(Brushes.White, 1);

            //        //dc.DrawLine(pen, av_rect.TopLeft, av_rect.BottomLeft);
            //        dc.DrawRectangle(
            //                brush,
            //                pen,
            //                av_rect);
            //    }
            //},null);

            var dtrl = _debug_dropRects; //await _dropBehavior.GetDropTargetRectsAsync();
            foreach (var debugRect in dtrl) {
                bool isDropIdx = dtrl.IndexOf(debugRect) == _dropIdx;

                var av_rect = debugRect.ToAvRect();
                IBrush brush = isDropIdx ? Brushes.Blue : new SolidColorBrush(_debugColor);
                IPen pen = new Pen(Brushes.White, 1);

                //dc.DrawLine(pen, av_rect.TopLeft, av_rect.BottomLeft);
                dc.DrawRectangle(
                        brush,
                        pen,
                        av_rect);
            }
        }
        #endregion
    }
}
