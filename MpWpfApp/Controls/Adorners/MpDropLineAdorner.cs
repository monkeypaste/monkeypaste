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

        #endregion

        #region Properties
        public Point[] Points { get; set; } = new Point[2];

        public Brush Color { get; set; } = Brushes.Red;

        public double Thickness { get; set; } = 1.5;

        public DashStyle DashStyle { get; set; }

        public bool IsShowing { get; set; } = false;

        public bool IsDebugMode { get; set; } = false;

        public List<Rect> DebugRects { get; set; } = new List<Rect>();

        public int DropIdx { get; set; } = -1;

        private Pen pen {
            get {
                return new Pen(Color, Thickness) { DashStyle = this.DashStyle };
            }
        }
        #endregion

        #region Public Methods
        public MpDropLineAdorner(UIElement uie) : base(uie) {
            DashStyle = DashStyles.Dash;
            _debugColor = MpHelpers.Instance.GetRandomColor();
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            if (MpClipTrayViewModel.Instance.IsLoadingMore) {
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
                drawingContext.DrawLine(pen, Points[0], Points[1]);
            } else if(!IsDebugMode) {
                Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
