using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class ZoomBorder : Border {
        // from https://stackoverflow.com/a/6782715/105028
        private UIElement child = null;
        private Point tt_origin;
        private Point mp_start;

        private TranslateTransform GetTranslateTransform(UIElement element) {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element) {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child {
            get { return base.Child; }
            set {
                if (value != null && value != this.Child) {
                    this.Initialize(value);
                }
                base.Child = value;
            }
        }

        public void Initialize(UIElement element) {
            this.child = element;
            if (child != null) {
                //TransformGroup group = new TransformGroup();
                //ScaleTransform st = new ScaleTransform();
                //group.Children.Add(st);
                //TranslateTransform tt = new TranslateTransform();
                //group.Children.Add(tt);
                //child.RenderTransform = group;
                //child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.PreviewMouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
            }
        }

        public void Reset() {
            if (child != null) {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        public void Translate(double x, double y) {
            var tt = GetTranslateTransform(child);
            tt.X = tt_origin.X - x;
            tt.Y = tt_origin.Y - y;
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (child != null) {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4)) {
                    return;
                }
                Point relative = e.GetPosition(child);
                double absoluteX;
                double absoluteY;

                absoluteX = relative.X * st.ScaleX + tt.X;
                absoluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;

                double zoomCorrected = zoom * st.ScaleX; 
                st.ScaleX += zoomCorrected; 
                st.ScaleY += zoomCorrected;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (child != null) {
                var tt = GetTranslateTransform(child);
                mp_start = e.GetPosition(this);
                tt_origin = new Point(tt.X, tt.Y);
                this.Cursor = System.Windows.Input.Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (child != null) {
                child.ReleaseMouseCapture();
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            this.Reset();
        }

        private void child_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (child != null) {
                if (child.IsMouseCaptured) {
                    var tt = GetTranslateTransform(child);
                    Vector v = mp_start - e.GetPosition(this);
                    tt.X = tt_origin.X - v.X;
                    tt.Y = tt_origin.Y - v.Y;
                }
            }
        }

        #endregion

        #region Bg Grid

        public Brush GridLineBrush { get; set; } = Brushes.LightBlue;
        public double GridLineThickness { get; set; } = 1;

        public Brush OriginBrush { get; set; } = Brushes.Cyan;
        public double OriginThickness { get; set; } = 3;

        public int GridLineSpacing { get; set; } = 35;

        #region ShowGrid DependencyProperty

        public bool ShowGrid {
            get { return (bool)GetValue(ShowGridProperty); }
            set { SetValue(ShowGridProperty, value); }
        }

        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(
                "ShowGrid", typeof(bool),
                typeof(ZoomBorder),
                new FrameworkPropertyMetadata(default(bool)));

        #endregion


        DispatcherTimer _timer = null;
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if(_timer == null) {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(50);
                _timer.Tick += (s, e) => InvalidateVisual();

                _timer.Start();
            }
            if(ShowGrid) {
                DrawGrid(dc);
            }
        }

        private void DrawGrid(DrawingContext dc) {
            Point offset = new Point(10, 10);

            int HorizontalGridLineCount = (int)(RenderSize.Width / GridLineSpacing);
            int VerticalGridLineCount = (int)(RenderSize.Height / GridLineSpacing);

            double xStep = RenderSize.Width / HorizontalGridLineCount;
            double yStep = RenderSize.Height / VerticalGridLineCount;
            double curX = 0;
            double curY = 0;

            for (int x = 0; x < HorizontalGridLineCount; x++) {
                Point p1 = new Point(curX, 0);
                p1 = (Point)(p1 - offset);
                Point p2 = new Point(curX, RenderSize.Height);
                p2 = (Point)(p2 - offset);

                bool isOrigin = x == (int)(HorizontalGridLineCount / 2);
                if (isOrigin) {
                    dc.DrawLine(new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curX += xStep;
            }

            for (int y = 0; y < VerticalGridLineCount; y++) {
                Point p1 = new Point(0, curY);
                p1 = (Point)(p1 - offset);
                Point p2 = new Point(RenderSize.Width, curY);
                p2 = (Point)(p2 - offset);

                bool isOrigin = y == (int)(VerticalGridLineCount / 2);
                if (isOrigin) {
                    dc.DrawLine(new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curY += yStep;
            }
        }

        #endregion
    }
}
