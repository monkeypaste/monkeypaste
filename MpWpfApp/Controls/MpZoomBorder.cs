using MonkeyPaste;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpZoomBorder : Border {
        // from https://stackoverflow.com/a/6782715/105028
        #region Private Variables
        private DispatcherTimer _renderTimer = null;

        //private UIElement Child = null;

        private Point tt_origin;
        private Point mp_start;
                
        private Point grid_offset {
            get {
                if(Child == null) {
                    return new Point();
                }
                var tt = GetTranslateTransform(Child);
                return new Point(tt.X, tt.Y);
            }
        }

        #endregion

        #region Statics

        public static bool IsTranslating { get; private set; } = false;

        #endregion

        #region Properties

        #region DesignerItem DependencyProperty

        public MpIDesignerItemSettingsViewModel DesignerItem {
            get { return (MpIDesignerItemSettingsViewModel)GetValue(DesignerItemProperty); }
            set { SetValue(DesignerItemProperty, value); }
        }

        public static readonly DependencyProperty DesignerItemProperty =
            DependencyProperty.Register(
                "DesignerItem", typeof(MpIDesignerItemSettingsViewModel),
                typeof(MpZoomBorder),
                new FrameworkPropertyMetadata { 
                    DefaultValue = default(MpIDesignerItemSettingsViewModel),
                    PropertyChangedCallback = (sender,e) => {
                        var zb = sender as MpZoomBorder;
                        zb.Reset();
                    }
                });

        #endregion               

        #region ShowGrid DependencyProperty

        public bool ShowGrid {
            get { return (bool)GetValue(ShowGridProperty); }
            set { SetValue(ShowGridProperty, value); }
        }

        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(
                "ShowGrid", typeof(bool),
                typeof(MpZoomBorder),
                new FrameworkPropertyMetadata(default(bool)));

        #endregion

        #region Bg Grid Properties

        public double MinScale { get; set; } = 0.1;
        public double MaxScale { get; set; } = 3;

        public Brush GridLineBrush { get; set; } = Brushes.LightBlue;
        public double GridLineThickness { get; set; } = 1;

        public Brush OriginBrush { get; set; } = Brushes.Cyan;
        public double OriginThickness { get; set; } = 3;

        public int GridLineSpacing { get; set; } = 35;

        #endregion

        //public override UIElement Child {
        //    get { return base.Child; }
        //    set {
        //        if (value != null && value != this.Child) {
        //            this.Initialize(value);
        //        }
        //        base.Child = value;
        //    }
        //}

        #endregion

        #region Public Methods

        public void Initialize(UIElement element) {
            this.Child = element;
            if (Child != null) {
                Child.PreviewMouseWheel += child_MouseWheel;
                Child.PreviewMouseLeftButtonDown += child_PreviewMouseLeftButtonDown;
                Child.MouseLeftButtonUp += child_MouseLeftButtonUp;
                Child.MouseMove += child_MouseMove;
                Child.PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
            child_PreviewMouseLeftButtonDown(Child, e);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            e.Handled = true;
            child_MouseLeftButtonUp(Child, e);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e) {
            base.OnMouseMove(e);
            e.Handled = true;
            child_MouseMove(Child, e);
        }
        public void Reset() {
            MpPoint scale = new MpPoint(1, 1);
            MpPoint offset = new MpPoint();
            if(DesignerItem != default) {
                scale = new MpPoint(DesignerItem.ScaleX, DesignerItem.ScaleY);
                offset = new MpPoint(DesignerItem.TranslateOffsetX, DesignerItem.TranslateOffsetY);
            }

            if (Child != null) {
                // reset zoom
                var st = GetScaleTransform(Child);
                st.ScaleX = scale.X;
                st.ScaleY = scale.Y;

                // reset pan
                var tt = GetTranslateTransform(Child);
                tt.X = offset.X;
                tt.Y = offset.Y;
            }
        }

        public void Translate(double x, double y) {
            // NOTE to be used by DesignerItem Drop Behavior
            var tt = GetTranslateTransform(Child);
            tt.X = tt_origin.X - x;
            tt.Y = tt_origin.Y - y;
        }

        #endregion

        #region Protected Methods

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if (_renderTimer == null) {
                _renderTimer = new DispatcherTimer();
                _renderTimer.Interval = TimeSpan.FromMilliseconds(50);
                _renderTimer.Tick += (s, e) => InvalidateVisual();

                _renderTimer.Start();
            }
            if (ShowGrid) {
                DrawGrid(dc);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            base.OnMouseWheel(e);

            child_MouseWheel(Child,e);
        }

        #endregion

        #region Private Methods

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (Child != null) {
                var st = GetScaleTransform(Child);
                var tt = GetTranslateTransform(Child);

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < MinScale || st.ScaleY < MinScale)) {
                    return;
                }
                Point relative = e.GetPosition(Child);
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


                st.ScaleX = Math.Max(MinScale, st.ScaleX);
                st.ScaleY = Math.Max(MinScale, st.ScaleY);

                if (DesignerItem != null) {
                    DesignerItem.ScaleX = st.ScaleX;
                    DesignerItem.ScaleY = st.ScaleY;
                    if (DataContext is MpActionCollectionViewModel acvm) {
                        acvm.HasModelChanged = true;
                    }
                }
            }
        }
        
        private void child_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (Child != null && !MpMoveBehavior.IsAnyMoving) {
                var tt = GetTranslateTransform(Child);
                mp_start = e.GetPosition(this);
                tt_origin = new Point(tt.X, tt.Y);
                MpCursor.SetCursor(DataContext, MpCursorType.Hand);
                bool result = CaptureMouse();
                if(!result) {
                    var capturer = Mouse.Captured;
                    Debugger.Break();
                } else {
                    IsTranslating = true;
                    e.Handled = true;
                }
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Child != null) {
                IsTranslating = false;
                ReleaseMouseCapture();
                MpCursor.UnsetCursor(DataContext);
                if(DataContext is MpActionCollectionViewModel acvm) {
                    acvm.HasModelChanged = true;
                }
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            this.Reset();
        }

        private void child_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (Child != null) {
                if (IsTranslating) {
                    var tt = GetTranslateTransform(Child);
                    Vector v = mp_start - e.GetPosition(this);


                    tt.X = tt_origin.X - v.X;
                    tt.Y = tt_origin.Y - v.Y;

                    if (DesignerItem != null) {
                        DesignerItem.TranslateOffsetX = tt.X;
                        DesignerItem.TranslateOffsetY = tt.Y;
                    }
                } 
            }
        }

        #endregion

        private TranslateTransform GetTranslateTransform(UIElement element) {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element) {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }
        
        private void DrawGrid(DrawingContext dc) {
            Point offset = new Point();// (grid_offset.ToMpPoint() * -1).ToWpfPoint(); //new Point(10, 10);

            var st = GetScaleTransform(Child);
            int HorizontalGridLineCount = (int)((RenderSize.Width / GridLineSpacing) * (1/st.ScaleX));
            int VerticalGridLineCount = (int)((RenderSize.Height / GridLineSpacing) * (1/st.ScaleY));

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
                    DrawLine(dc,new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    DrawLine(dc,new Pen(GridLineBrush, GridLineThickness), p1, p2);
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
                    DrawLine(dc,new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    DrawLine(dc,new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curY += yStep;
            }
        }

        private void DrawLine(DrawingContext dc, Pen p, Point p1, Point p2) {
            var offset = new Point(); //grid_offset;
            p1.X -= offset.X;
            p1.Y -= offset.Y;

            p2.X -= offset.X;
            p2.Y -= offset.Y;

            dc.DrawLine(p, p1, p2);
        }

        #endregion

    }
}
