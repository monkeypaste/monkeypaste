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
    public class MpDropLineAdorner : Adorner {
        #region Private Variables

        private Pen _pen;

        private Color _debugColor;

        private MpIContentDropTarget _dropBehavior;
        #endregion

        #region Properties
        
        public Line DropLine { 
            get {
                var line = new Line();
                if(DropIdx < 0 || _dropBehavior == null) {
                    return line;
                }
                Rect dropRect = DebugRects[DropIdx];
                if(_dropBehavior.AdornerOrientation == Orientation.Vertical) {
                    line.X1 = dropRect.Left + (dropRect.Width / 2);
                    line.Y1 = dropRect.Top;
                    line.X2 = line.X1;
                    line.Y2 = dropRect.Bottom;
                } else {
                    line.X1 = dropRect.Left;
                    line.X2 = dropRect.Right;
                    if (DropIdx == 0) {
                        line.Y1 = line.Y2 = dropRect.Bottom;
                    } else if (DropIdx == DebugRects.Count - 1) {
                        line.Y1 = line.Y2 = dropRect.Top;
                    } else {
                        line.Y1 = line.Y2 = dropRect.Top + (dropRect.Height / 2);
                    }
                }
                return line;
            }        
        }

        public bool IsShowing => DropIdx >= 0;

        public bool IsDebugMode { get; set; } = false;

        public List<Rect> DebugRects { 
            get {
                if(_dropBehavior == null) {
                    return new List<Rect>();
                }
                return _dropBehavior.GetDropTargetRects();
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
        public MpDropLineAdorner(UIElement uie, MpIContentDropTarget dropBehavior) : base(uie) {
            _dropBehavior = dropBehavior;

            _debugColor = MpHelpers.Instance.GetRandomColor();
            _debugColor.A = 50;
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            if (MpClipTrayViewModel.Instance.IsBusy) {
                return;
            }
            if(IsDebugMode) {
                Visibility = Visibility.Visible;
                foreach(var debugRect in DebugRects) {
                    if(DebugRects.IndexOf(debugRect) == DropIdx) {
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
            }

            if(IsShowing) {
                Visibility = Visibility.Visible;
                drawingContext.DrawLine(
                    new Pen(Brushes.Red, 1.5) { DashStyle = DashStyles.Dash },
                    new Point(DropLine.X1,DropLine.Y1),
                    new Point(DropLine.X2, DropLine.Y2));
            } else if(!IsDebugMode) {
                Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
