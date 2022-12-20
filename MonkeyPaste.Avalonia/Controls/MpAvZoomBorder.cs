using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvZoomBorder : Border {
        // from https://stackoverflow.com/a/6782715/105028
        #region Private Variables
        private DispatcherTimer _renderTimer = null;

        //private UIElement Child = null;

        private MpPoint tt_origin;
        private MpPoint mp_start;
                
        private MpPoint grid_offset {
            get {
                if(Child == null) {
                    return new MpPoint();
                }
                var tt = GetTranslateTransform(Child);
                return new MpPoint(tt.X, tt.Y);
            }
        }

        #endregion

        #region Statics
        static MpAvZoomBorder() {
            IsEnabledProperty.Changed.AddClassHandler<MpAvZoomBorder>((x, y) => HandleDesignerItemChanged(x, y));
        }
        public static bool IsTranslating { get; private set; } = false;

        #endregion

        #region Properties

        #region DesignerItem AvaloniaProperty
        public MpIDesignerSettingsViewModel DesignerItem {
            get { return (MpIDesignerSettingsViewModel)GetValue(DesignerItemProperty); }
            set { SetValue(DesignerItemProperty, value); }
        }

        public static readonly AttachedProperty<MpIDesignerSettingsViewModel> DesignerItemProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpIDesignerSettingsViewModel>(
                "DesignerItem",
                null,
                false);

        private static void HandleDesignerItemChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(element is MpAvZoomBorder zb) {
                zb.Reset();
            }
        }

        #endregion

        #region ShowGrid AvaloniaProperty
        public bool ShowGrid {
            get { return (bool)GetValue(ShowGridProperty); }
            set { SetValue(ShowGridProperty, value); }
        }

        public static readonly AttachedProperty<bool> ShowGridProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "ShowGrid",
                true,
                false);

        #endregion

        #region Bg Grid Properties

        public double MinScale { get; set; } = 0.1;
        public double MaxScale { get; set; } = 3;

        public IBrush GridLineBrush { get; set; } = Brushes.LightBlue;
        public double GridLineThickness { get; set; } = 1;

        public IBrush OriginBrush { get; set; } = Brushes.Cyan;
        public double OriginThickness { get; set; } = 3;

        public int GridLineSpacing { get; set; } = 35;

        #endregion

        #endregion

        #region Public Methods

        public void Initialize(IControl element) {
            this.Child = element;
            if (Child != null) {
                Child.PointerWheelChanged += child_MouseWheel;
                Child.PointerPressed += child_PreviewMouseLeftButtonDown;
                Child.PointerReleased += child_MouseLeftButtonUp;
                Child.PointerMoved += child_MouseMove;
                //Child.PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            e.Handled = true;
            child_PreviewMouseLeftButtonDown(Child, e);
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            e.Handled = true;
            child_MouseLeftButtonUp(Child, e);
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);
            e.Handled = true;
            child_MouseMove(Child, e);
        }
        public void Reset() {
            double scale = 1.0d;
            MpPoint offset = new MpPoint();
            if(DesignerItem != null) {
                scale = DesignerItem.Scale;
                offset = new MpPoint(DesignerItem.TranslateOffsetX, DesignerItem.TranslateOffsetY);
            }

            if (Child != null) {
                // reset zoom
                var st = GetScaleTransform(Child);
                st.ScaleX = scale;
                st.ScaleY = scale;

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

        public override void Render(DrawingContext dc) {
            base.Render(dc);

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

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e) {
            base.OnPointerWheelChanged(e);

            child_MouseWheel(Child,e);
        }

        #endregion

        #region Private Methods

        #region Child Events

        private void child_MouseWheel(object sender, PointerWheelEventArgs e) {
            if(Child == null || DesignerItem == null) {
                return;
            }
            var st = GetScaleTransform(Child);
            double scale = DesignerItem.Scale;
            var tt = GetTranslateTransform(Child);

            double zoom = e.Delta.Y > 0 ? .2 : -.2;
            if (!(e.Delta.Y > 0) && scale < MinScale) {
                return;
            }
            scale += zoom;

            MpPoint relative = e.GetPosition(Child).ToPortablePoint();
            double absolute;
            double absoluteY;

            absolute = relative.X * scale + tt.X;
            absoluteY = relative.Y * scale + tt.Y;

            tt.X = absolute - relative.X * scale;
            tt.Y = absoluteY - relative.Y * scale;

            double zoomCorrected = zoom * scale;
            scale += zoomCorrected;

            DesignerItem.Scale = Math.Min(MaxScale,Math.Max(MinScale, scale));
            //DesignerItem.Scale = st.ScaleX;
            if (DataContext is MpAvTriggerCollectionViewModel acvm) {
                //acvm.HasModelChanged = true;
            }
        }
        
        private void child_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e) {
            if (Child != null && !MpAvMoveExtension.IsAnyMoving) {
                var tt = GetTranslateTransform(Child);
                mp_start = e.GetPosition(this).ToPortablePoint();
                tt_origin = new MpPoint(tt.X, tt.Y);
                MpPlatformWrapper.Services.Cursor.SetCursor(Child, MpCursorType.Hand);

                e.Pointer.Capture(this);
                if(e.Pointer.Captured != this) {
                    var capturer = e.Pointer.Captured;
                    Debugger.Break();
                } else {
                    IsTranslating = true;
                    e.Handled = true;
                }
            }
        }

        private void child_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e) {
            if (Child != null) {
                IsTranslating = false;
                e.Pointer.Capture(null);
                MpPlatformWrapper.Services.Cursor.UnsetCursor(DataContext);
                if(DataContext is MpAvTriggerCollectionViewModel acvm) {
                    acvm.HasModelChanged = true;
                }
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, PointerPressedEventArgs e) {
            this.Reset();
        }

        private void child_MouseMove(object sender, PointerEventArgs e) {
            if (Child != null) {
                if (IsTranslating) {
                    var tt = GetTranslateTransform(Child);
                    var v = mp_start - e.GetPosition(this).ToPortablePoint();


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

        private TranslateTransform GetTranslateTransform(IControl element) {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(IControl element) {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }
        
        private void DrawGrid(DrawingContext dc) {
            double w = Bounds.Width;
            double h = Bounds.Height;

            double offset_x = DesignerItem.TranslateOffsetX;
            double offset_y = DesignerItem.TranslateOffsetY;

            var st = GetScaleTransform(Child);
            int HorizontalGridLineCount = (int)((w / GridLineSpacing) * (1/st.ScaleX));
            int VerticalGridLineCount = (int)((h / GridLineSpacing) * (1/st.ScaleY));

            int major_count = 5;
            double major_thickness = 2;
            double minor_thickness = 0.5;

            double xStep = w / HorizontalGridLineCount;
            double yStep = h / VerticalGridLineCount;

            double curX = 0;
            double curY = 0;

            for (int x = 0; x < HorizontalGridLineCount; x++) {
                MpPoint p1 = new MpPoint(curX, 0);
                p1.X += offset_x;
                MpPoint p2 = new MpPoint(curX, h);
                p2.X += offset_x;

                if (x == 0) {
                    int fill_count = 0;
                    var fill_p = p1 - new MpPoint(xStep, 0);
                    while (fill_p.X > 0) {
                        fill_count++;
                        double vert_fill_line_thickness = fill_count % major_count == 0 ? major_thickness : minor_thickness;
                        DrawLine(dc, new Pen(GridLineBrush, vert_fill_line_thickness), fill_p, fill_p + new MpPoint(0, h));
                        fill_p -= new MpPoint(xStep, 0);
                    }
                } else if (x == HorizontalGridLineCount - 1) {
                    int fill_count = 0;
                    var fill_p = p1 + new MpPoint(xStep, 0);
                    while (fill_p.X < w) {
                        fill_count++;
                        double vert_fill_line_thickness = fill_count % major_count == 0 ? major_thickness : minor_thickness;
                        DrawLine(dc, new Pen(GridLineBrush, vert_fill_line_thickness), fill_p, fill_p + new MpPoint(0, h));
                        fill_p += new MpPoint(xStep, 0);
                    }
                }

                double vert_line_thickness = x % major_count == 0 ? major_thickness : minor_thickness;
                DrawLine(dc, new Pen(GridLineBrush, vert_line_thickness), p1, p2);

                curX += xStep;
            }

            for (int y = 0; y < VerticalGridLineCount; y++) {
                MpPoint p1 = new MpPoint(0, curY);
                p1.Y += offset_y;
                MpPoint p2 = new MpPoint(w, curY);
                p2.Y += offset_y;

                if (y == 0) {
                    int fill_count = 0;
                    var fill_p = p1 - new MpPoint(0, yStep);
                    while (fill_p.Y > 0) {
                        fill_count++;
                        double horiz_fill_line_thickness = fill_count % major_count == 0 ? major_thickness : minor_thickness;
                        DrawLine(dc, new Pen(GridLineBrush, horiz_fill_line_thickness), fill_p, fill_p + new MpPoint(w, 0));
                        fill_p -= new MpPoint(0, yStep);
                    }
                } else if (y == VerticalGridLineCount - 1) {
                    int fill_count = 0;
                    var fill_p = p1 + new MpPoint(0, yStep);
                    while (fill_p.Y < h) {
                        fill_count++;
                        double horiz_fill_line_thickness = fill_count % major_count == 0 ? major_thickness : minor_thickness;
                        DrawLine(dc, new Pen(GridLineBrush, horiz_fill_line_thickness), fill_p, fill_p + new MpPoint(w, 0));
                        fill_p += new MpPoint(0, yStep);
                    }
                }


                double horiz_line_thickness = y % major_count == 0 ? major_thickness : minor_thickness;
                DrawLine(dc, new Pen(GridLineBrush, horiz_line_thickness), p1, p2);

                curY += yStep;
            }
        }

        private void DrawLine(DrawingContext dc, Pen p, MpPoint p1, MpPoint p2) {
            var offset = new MpPoint(); //grid_offset;
            p1.X -= offset.X;
            p1.Y -= offset.Y;

            p2.X -= offset.X;
            p2.Y -= offset.Y;

            dc.DrawLine(p, p1.ToAvPoint(), p2.ToAvPoint());
        }

        #endregion

    }
}
