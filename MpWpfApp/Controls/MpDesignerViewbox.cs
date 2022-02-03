using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpDesignerViewbox : Viewbox {
        private DispatcherTimer _timer;

        #region Properties

        public Brush GridLineBrush { get; set; } = Brushes.LightBlue;
        public double GridLineThickness { get; set; } = 1;

        public Brush OriginBrush { get; set; } = Brushes.Cyan;
        public double OriginThickness { get; set; } = 3;

        public int GridLineSpacing { get; set; } = 35;


        #endregion

        public override void EndInit() {
            base.EndInit();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;

            _timer.Start();
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            if(DataContext is MpTriggerActionViewModelBase tavm) {
                if(tavm.Parent.IsAnySelected) {
                    DrawGrid(drawingContext);
                }
            }
        }

        private void DrawGrid(DrawingContext dc) {
            int HorizontalGridLineCount = (int)(RenderSize.Width / GridLineSpacing);

            double xStep = RenderSize.Width / HorizontalGridLineCount;
            double curX = 0;
            for (int x = 0; x < HorizontalGridLineCount; x++) {
                Point p1 = new Point(curX, 0);
                Point p2 = new Point(curX, RenderSize.Height);

                bool isOrigin = x == (int)(HorizontalGridLineCount / 2);
                if (isOrigin) {
                    dc.DrawLine(new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curX += xStep;
            }

            int VerticalGridLineCount = (int)(RenderSize.Height / GridLineSpacing);
            double yStep = RenderSize.Height / VerticalGridLineCount;
            double curY = 0;
            for (int y = 0; y < VerticalGridLineCount; y++) {
                Point p1 = new Point(0, curY);
                Point p2 = new Point(RenderSize.Width, curY);

                bool isOrigin = y == (int)(VerticalGridLineCount / 2);
                if (isOrigin) {
                    dc.DrawLine(new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curY += yStep;
            }
        }

        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }
    }
}
