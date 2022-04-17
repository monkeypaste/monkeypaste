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

namespace MpWpfApp {
    public class MpDropShapeAdorner : Adorner {
        #region Private Variables
        private bool _isDash = true;

        private Color _debugColor;

        private MpIContentDropTarget _dropBehavior;

        #endregion

        #region Properties
        
        //public MpLine DropLine { 
        //    get {
        //        if(_dropBehavior is MpContentViewDropBehavior cvdb) {
        //            _isDash = false;
        //            MpConsole.WriteLine("ContentView DropIdx: " + cvdb.DropIdx);

        //            var dltp = (cvdb.RelativeToElement as RichTextBox).Document.ContentStart.GetPositionAtOffset(cvdb.DropIdx);
        //            var dltp_rect = dltp.GetCharacterRect(LogicalDirection.Forward);
        //            return new MpLine(dltp_rect.Left, dltp_rect.Top, dltp_rect.Left, dltp_rect.Bottom);
        //        }

        //        if(DropIdx < 0 || 
        //           _dropBehavior == null ||
        //           DropIdx >= DropRects.Count) {
        //            return new MpLine();
        //        }
        //        Rect dropRect = DropRects[DropIdx];
        //        if(_dropBehavior.AdornerOrientation == Orientation.Vertical) {
        //            //tray or rtb view vertical drop line
        //            double x = dropRect.Left + (dropRect.Width / 2) + 2;
        //            if(DropIdx > 0 && DropIdx < DropRects.Count - 1) {
        //                // NOTE due to margin/padding issues calculate mid-point between drop rects and 
        //                // do not derive from singluar drop rect
        //                //x = dropRect.Left + 5;// ((DropRects[DropIdx + 1].Left - dropRect.Left) / 2);
        //            }
        //            return new MpLine(x,dropRect.Top,x,dropRect.Bottom);
        //        }

        //        double x1 = dropRect.Left;
        //        double x2 = dropRect.Right;
        //        double y = dropRect.Bottom - MpContentListDropBehavior.TargetMargin;
        //        if(DropIdx == DropRects.Count - 1) {
        //            y = dropRect.Top;
        //        }
        //        return new MpLine(x1, y, x2, y);
        //    }        
        //}

        public bool IsShowing => DropIdx >= 0;

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
        public MpDropShapeAdorner(UIElement uie, MpIContentDropTarget dropBehavior) : base(uie) {
            _dropBehavior = dropBehavior;
            _debugColor = MpWpfColorHelpers.GetRandomColor();
            _debugColor.A = 50;
            
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            //if(!_isTargetSelected && IsDebugMode) {
            //    return;
            //}

            if(IsDebugMode && 
               DropRects != null) {
                Visibility = Visibility.Visible;
                foreach(var debugRect in DropRects) {
                    if(DropRects.IndexOf(debugRect) == DropIdx) {
                        drawingContext.DrawRectangle(
                            Brushes.Blue,
                            new Pen(Brushes.Orange, 1),
                            debugRect);
                    } else {
                        drawingContext.DrawRectangle(
                            new SolidColorBrush(_debugColor),
                            new Pen(Brushes.Orange, 1),
                            debugRect);
                    }                    
                }
            } else {
                if (MpClipTrayViewModel.Instance.IsBusy || 
                   !MpDragDropManager.IsDragAndDrop || 
                   !_dropBehavior.IsDropEnabled ||
                   DropRects == null) {
                    Visibility = Visibility.Hidden;
                    return;
                }
            }

            if(IsShowing) {
                var pen = new Pen(Brushes.Red, 1.5) { DashStyle = _isDash ? DashStyles.Dash : DashStyles.Solid };
                Visibility = Visibility.Visible;
                var dropShape = _dropBehavior.GetDropTargetAdornerShape();
                if(dropShape is MpLine dl) {
                    drawingContext.DrawLine(
                        pen,
                        dl.P1.ToWpfPoint(),
                        dl.P2.ToWpfPoint());
                } else if(dropShape is MpEllipse de) {
                    drawingContext.DrawEllipse(
                        Brushes.Transparent,
                        pen,
                        de.Center.ToWpfPoint(),
                        de.Size.Width / 2,
                        de.Size.Height / 2);
                } else if(dropShape is MpRect dr) {
                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        pen,
                        dr.ToWpfRect());
                }
                
            } else if(!IsDebugMode) {
                Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
