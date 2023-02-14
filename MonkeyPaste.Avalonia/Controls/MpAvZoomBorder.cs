using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvZoomBorder : Border {
        // from https://stackoverflow.com/a/6782715/105028
        #region Private Variables
        private DispatcherTimer _renderTimer = null;

        private MpPoint mp_start;

        #endregion


        #region Statics
        static MpAvZoomBorder() {
            //IsEnabledProperty.Changed.AddClassHandler<MpAvZoomBorder>((x, y) => HandleDesignerItemChanged(x, y));
        }
        public static bool IsTranslating { get; private set; } = false;

        #endregion

        #region Properties

        #region DesignerItem AvaloniaProperty
        public MpIDesignerSettingsViewModel DesignerItem => DataContext as MpIDesignerSettingsViewModel;

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

        #region MinScale AvaloniaProperty
        public double MinScale {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        public static readonly AttachedProperty<double> MinScaleProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "MinScale",
                0.1,
                false);

        #endregion

        #region MaxScale AvaloniaProperty
        public double MaxScale {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        public static readonly AttachedProperty<double> MaxScaleProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "MaxScale",
                3.0d,
                false);

        #endregion

        #region Bg Grid Properties


        public IBrush GridLineBrush { get; set; } = Brushes.LightBlue;
        public double GridLineThickness { get; set; } = 1;

        public IBrush OriginBrush { get; set; } = Brushes.Cyan;
        public double OriginThickness { get; set; } = 3;

        public int GridLineSpacing { get; set; } = 35;

        #endregion

        #endregion

        #region Public Methods

        public void Initialize(Control element) {
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
            if (DesignerItem == null) {
                return;
            }

            DesignerItem.Scale = 1.0d;
            DesignerItem.TranslateOffsetX = 0;
            DesignerItem.TranslateOffsetY = 0;
        }

        public void Zoom(double scaleDelta, MpPoint relative_anchor) {
            if (DesignerItem == null) {
                return;
            }
            double scale = DesignerItem.Scale;
            double tx = DesignerItem.TranslateOffsetX;
            double ty = DesignerItem.TranslateOffsetY;

            if (scale < MinScale) {
                return;
            }
            scale += scaleDelta;

            double absolute;
            double absoluteY;

            absolute = relative_anchor.X * scale + tx;
            absoluteY = relative_anchor.Y * scale + ty;

            tx = absolute - relative_anchor.X * scale;
            ty = absoluteY - relative_anchor.Y * scale;

            double zoomCorrected = scaleDelta * scale;
            scale += zoomCorrected;

            DesignerItem.Scale = Math.Min(MaxScale, Math.Max(MinScale, scale));
            DesignerItem.TranslateOffsetX = tx;
            DesignerItem.TranslateOffsetY = ty;

            if (DataContext is MpAvTriggerCollectionViewModel acvm) {
                acvm.HasModelChanged = true;
            }
        }
        public void TranslateOrigin(double x, double y) {
            if (DesignerItem == null) {
                return;
            }
            // NOTE to be used by DesignerItem Drop Behavior
            //var tt = GetTranslateTransform(Child);
            DesignerItem.TranslateOffsetX -= x;
            DesignerItem.TranslateOffsetY -= y;
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

            child_MouseWheel(Child, e);
        }

        #endregion

        #region Private Methods

        #region Child Events

        private void child_MouseWheel(object sender, PointerWheelEventArgs e) {
            if (Child == null || DesignerItem == null) {
                return;
            }
            double zoom = e.Delta.Y > 0 ? .2 : -.2;
            Zoom(zoom, e.GetPosition(Child).ToPortablePoint());
        }

        private void child_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e) {
            if (Child != null && !MpAvMoveExtension.IsAnyMoving) {
                mp_start = e.GetPosition(this).ToPortablePoint();
                e.Pointer.Capture(this);
                if (e.Pointer.Captured != this) {
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
                if (DataContext is MpAvTriggerCollectionViewModel acvm) {
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
                    //var tt = GetTranslateTransform(Child);
                    var mp = e.GetPosition(this).ToPortablePoint();
                    var v = mp_start - mp;
                    TranslateOrigin(v.X, v.Y);
                    mp_start = mp;
                }
            }
        }

        #endregion

        //private TranslateTransform GetTranslateTransform(IControl element) {
        //    return (TranslateTransform)((TransformGroup)element.RenderTransform)
        //      .Children.First(tr => tr is TranslateTransform);
        //}

        //private ScaleTransform GetScaleTransform(IControl element) {
        //    return (ScaleTransform)((TransformGroup)element.RenderTransform)
        //      .Children.First(tr => tr is ScaleTransform);
        //}

        private void DrawGrid(DrawingContext dc) {
            if (DesignerItem == null) {
                return;
            }

            var di = DesignerItem;

            double w = Bounds.Width;
            double h = Bounds.Height;

            double offset_x = di.TranslateOffsetX;
            double offset_y = di.TranslateOffsetY;

            //var st = GetScaleTransform(Child);
            int HorizontalGridLineCount = (int)((w / GridLineSpacing) * (1 / di.Scale));
            int VerticalGridLineCount = (int)((h / GridLineSpacing) * (1 / di.Scale));

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
