using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvZoomBorder : UserControl, MpIOverrideRender {
        // from https://stackoverflow.com/a/6782715/105028
        #region Private Variables
        private DispatcherTimer _render_timer = null;

        private MpPoint mp_start;

        #endregion

        #region Statics
        static MpAvZoomBorder() {
            //IsEnabledProperty.Changed.AddClassHandler<MpAvZoomBorder>((x, y) => HandleDesignerItemChanged(x, y));
        }
        public static bool IsTranslating { get; private set; } = false;

        #endregion

        #region Interfaces

        public bool IgnoreRender { get; set; }

        #endregion

        #region Properties

        public Control Child =>
            Content as Control;

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

        #region Arrow Properties

        #region Appearance

        #region WarningBrush1 Property
        public IBrush WarningBrush1 {
            get { return (IBrush)GetValue(WarningBrush1Property); }
            set { SetValue(WarningBrush1Property, value); }
        }

        public static readonly AttachedProperty<IBrush> WarningBrush1Property =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "WarningBrush1",
                Brushes.Yellow,
                false);
        #endregion

        #region WarningBrush2 Property
        public IBrush WarningBrush2 {
            get { return (IBrush)GetValue(WarningBrush2Property); }
            set { SetValue(WarningBrush2Property, value); }
        }

        public static readonly AttachedProperty<IBrush> WarningBrush2Property =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "WarningBrush2",
                Brushes.Black,
                false);
        #endregion

        #region TransitionLineDefaultBorderBrush Property
        public IBrush TransitionLineDefaultBorderBrush {
            get { return (IBrush)GetValue(TransitionLineDefaultBorderBrushProperty); }
            set { SetValue(TransitionLineDefaultBorderBrushProperty, value); }
        }

        public static readonly AttachedProperty<IBrush> TransitionLineDefaultBorderBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "TransitionLineDefaultBorderBrush",
                Brushes.White,
                false);
        #endregion

        #region TransitionLineHoverBorderBrush Property
        public IBrush TransitionLineHoverBorderBrush {
            get { return (IBrush)GetValue(TransitionLineHoverBorderBrushProperty); }
            set { SetValue(TransitionLineHoverBorderBrushProperty, value); }
        }

        public static readonly AttachedProperty<IBrush> TransitionLineHoverBorderBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "TransitionLineHoverBorderBrush",
                Brushes.Yellow,
                false);
        #endregion

        #region TransitionLineDisabledFillBrush Property
        public IBrush TransitionLineDisabledFillBrush {
            get { return (IBrush)GetValue(TransitionLineDisabledFillBrushProperty); }
            set { SetValue(TransitionLineDisabledFillBrushProperty, value); }
        }

        public static readonly AttachedProperty<IBrush> TransitionLineDisabledFillBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "TransitionLineDisabledFillBrush",
                Brushes.Red,
                false);
        #endregion

        #region TransitionLineEnabledFillBrush Property
        public IBrush TransitionLineEnabledFillBrush {
            get { return GetValue(TransitionLineEnabledFillBrushProperty); }
            set { SetValue(TransitionLineEnabledFillBrushProperty, value); }
        }

        public static readonly AttachedProperty<IBrush> TransitionLineEnabledFillBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "TransitionLineEnabledFillBrush",
                Brushes.Green,
                false);
        #endregion

        #endregion

        #region Layout
        public double TransitionLineThickness { get; set; } = 1;
        public double TipWidth { get; set; } = 10;
        public double TipLength { get; set; } = 20;
        public double TailWidth { get; set; } = 5;
        #endregion

        #endregion

        #endregion

        #region Public Methods

        public override void Render(DrawingContext ctx) {
            if (IgnoreRender) {
                return;
            }

            base.Render(ctx);
            if (ShowGrid) {
                DrawGrid(ctx);
            }
            if (DesignerItem == null) {
                return;
            }

            DrawItemEffects(ctx);
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

            if (scale < MinScale) {
                return;
            }
            scale += scaleDelta;
            double zoomCorrected = scaleDelta * scale;
            scale += zoomCorrected;

            DesignerItem.Scale = Math.Min(MaxScale, Math.Max(MinScale, scale));

            var t = new MpPoint(DesignerItem.TranslateOffsetX, DesignerItem.TranslateOffsetY) * scale;
            //MpPoint abs = (relative_anchor * scale) + t;
            //t = abs - relative_anchor * scale;
            DesignerItem.TranslateOffsetX = t.X;
            DesignerItem.TranslateOffsetY = t.Y;

            if (DataContext is MpAvTriggerCollectionViewModel acvm) {
                acvm.HasModelChanged = true;
            }
        }
        public void TranslateOrigin(double x, double y) {
            if (DesignerItem == null) {
                return;
            }
            // NOTE to be used by DesignerItem Drop Behavior
            DesignerItem.TranslateOffsetX -= x;
            DesignerItem.TranslateOffsetY -= y;
        }

        #endregion

        #region Protected Methods
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

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if (_render_timer == null) {
                _render_timer = new DispatcherTimer();
                _render_timer.Interval = TimeSpan.FromMilliseconds(50);
                _render_timer.Tick += (s, e) => {
                    Dispatcher.UIThread.Post(InvalidateVisual);
                };
            }

            _render_timer.Start();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            if (_render_timer == null) {
                return;
            }
            _render_timer.Stop();
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

        #region Grid Rendering
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

        #region Arrow Rendering

        private void DrawItemEffects(DrawingContext dc) {

            MpAvTriggerActionViewModelBase tavm = null;
            if (DataContext is MpAvTriggerCollectionViewModel tcvm) {
                tavm = tcvm.SelectedTrigger;
            }
            if (tavm == null) {
                return;
            }
            foreach (MpAvActionViewModelBase avm in tavm.SelfAndAllDescendants) {
                DrawActionShadow(dc, avm);

                var pavm = avm.ParentActionViewModel;
                if (pavm == null) {
                    // trigger has no arrow
                    continue;
                }

                MpRect cur_rect = GetTranslatedActionShapeRect(avm);
                MpRect parent_rect = GetTranslatedActionShapeRect(pavm);

                MpPoint tail = cur_rect.Centroid();
                MpPoint head = parent_rect.Centroid();

                var borderBrush = pavm.IsHovering ? TransitionLineHoverBorderBrush : TransitionLineDefaultBorderBrush;
                var fillBrush = GetArrowFillBrush(pavm, avm, head, tail);

                double end_adjust = Math.Sqrt(Math.Pow(avm.Width / 2, 2) + Math.Pow(avm.Height / 2, 2));
                DrawArrow(dc, head, tail, end_adjust, borderBrush, fillBrush);
            }
        }

        private Shape GetActionShape(MpAvActionViewModelBase avm) {
            if (this.GetVisualDescendant<ListBox>() is ListBox lb &&
                lb.ContainerFromItem(avm) is ListBoxItem lbi) {
                if (lbi.GetVisualDescendant<Shape>() is Shape avm_shape) {
                    return avm_shape;
                }
            }
            return null;
        }
        private MpRect GetTranslatedActionShapeRect(MpAvActionViewModelBase avm) {
            if (this.GetVisualDescendant<ListBox>() is ListBox lb &&
                GetActionShape(avm) is Shape s) {
                var s_rect = s.Bounds.ToPortableRect();
                var s_center = s.TranslatePoint(new Point(/*s.Bounds.Width / 2, s.Bounds.Height / 2*/), lb).Value.ToPortablePoint();
                s_center /= DesignerItem.Scale;
                s_rect.Move(s_center);
                return s_rect;
            }
            return MpRect.Empty;
        }
        private void DrawActionShadow(DrawingContext ctx, MpAvActionViewModelBase avm) {
            double scale = avm.Parent.Scale;
            MpPoint shadow_offset = new MpPoint(3, 3);// * scale;
            MpRect shape_rect = GetTranslatedActionShapeRect(avm);

            IBrush shadow_brush = new SolidColorBrush(Colors.Black, 0.1);

            Shape s = GetActionShape(avm);
            if (s == null) {
                return;
            }
            if (s is Ellipse el) {
                var r = el.Bounds.Size.ToPortableSize().ToPortablePoint() * 0.5;
                var center = shape_rect.Centroid() + shadow_offset;
                using (ctx.PushTransform(
                    Matrix.CreateScale(scale, scale) //*
                                                     //Matrix.CreateTranslation(center.X, center.Y)
                    )) {
                    //ctx.DrawEllipse(shadow_brush, new Pen(Brushes.Transparent), center.ToAvPoint(), r.X, r.Y);
                    ctx.DrawEllipse(shadow_brush, new Pen(Brushes.Transparent), center.ToAvPoint(), r.X, r.Y);
                }
            } else if (s is Polygon pg) {
                using (ctx.PushTransform(
                    Matrix.CreateScale(scale, scale) //*
                                                     //Matrix.CreateTranslation(origin.X, origin.Y)
                    )) {

                    var pg_trans = (pg.Bounds.ToPortableRect().Location + shape_rect.TopLeft + shadow_offset).ToAvPoint();
                    ctx.DrawGeometry(shadow_brush, new Pen(Brushes.Transparent), GetPointGeometry(pg.Points, pg_trans));
                }
            } else if (s is Rectangle r) {
                using (ctx.PushTransform(
                    Matrix.CreateScale(scale, scale) //*
                                                     //Matrix.CreateTranslation(origin.X, origin.Y)
                    )) {
                    //var rect = r.Bounds.ToPortableRect();
                    //rect.Move(origin);
                    //rect.Size.Width *= scale;
                    //rect.Size.Height *= scale;
                    var tl = shape_rect.TopLeft + shadow_offset;
                    ctx.DrawRectangle(shadow_brush, new Pen(Brushes.Transparent), new Rect(tl.ToAvPoint(), r.Bounds.Size));
                }
            } else {
                MpDebug.Break($"Unhandled shape type '{s.GetType()}'");
            }

        }
        private IBrush GetArrowFillBrush(MpAvActionViewModelBase pavm, MpAvActionViewModelBase avm, MpPoint pp, MpPoint p) {
            Color enabled_color = TransitionLineEnabledFillBrush.GetColor();
            Color disabled_color = TransitionLineDisabledFillBrush.GetColor();
            Color warning_color1 = WarningBrush1.GetColor();
            Color warning_color2 = WarningBrush2.GetColor();

            var parent_color = pavm.IsTriggerEnabled ? enabled_color : disabled_color;
            var cur_color = avm.IsTriggerEnabled ? enabled_color : disabled_color;

            var fillBrush = new LinearGradientBrush() {
                GradientStops = new GradientStops(),
                StartPoint = new RelativePoint(pp.ToAvPoint(), RelativeUnit.Absolute),
                EndPoint = new RelativePoint(p.ToAvPoint(), RelativeUnit.Absolute)
            };

            if (pavm.IsValid) {
                fillBrush.GradientStops.Add(new GradientStop(parent_color, 0));
                fillBrush.GradientStops.Add(new GradientStop(parent_color, 0.45d));
            } else {
                fillBrush.GradientStops.AddRange(GetGradientStripes(warning_color1, warning_color2, 0, 0.5, 7));
            }

            if (avm.IsValid) {
                fillBrush.GradientStops.Add(new GradientStop(cur_color, 0.55d));
                fillBrush.GradientStops.Add(new GradientStop(cur_color, 1.0d));
            } else {
                fillBrush.GradientStops.AddRange(GetGradientStripes(warning_color1, warning_color2, 0.5, 1, 7));
                fillBrush.Transform = new RotateTransform(-90);
            }

            return fillBrush;
        }

        private IEnumerable<GradientStop> GetGradientStripes(
            Color color1, Color color2, double start_offset, double end_offset, int count) {
            int altVal = 0;
            double offset_step = (end_offset - start_offset) / count;
            for (double cur_offset = start_offset; cur_offset < end_offset; cur_offset += offset_step) {
                Color cur_color = (altVal++ % 2) == 0 ? color1 : color2;
                yield return new GradientStop(cur_color, cur_offset);
                yield return new GradientStop(cur_color, cur_offset + offset_step);
            }
        }

        private void DrawArrow(
            DrawingContext ctx,
            MpPoint startPoint,
            MpPoint endPoint,
            double dw,
            IBrush borderBrush,
            IBrush fillBrush) {
            // compensate if actions are very close
            //dw = Math.Min(dw, (startPoint - endPoint).Length);
            double test1 = (startPoint - endPoint).Length;

            MpPoint direction = endPoint - startPoint;

            MpPoint normalizedDirection = direction;
            normalizedDirection.Normalize();

            startPoint += normalizedDirection * dw;
            endPoint -= normalizedDirection * dw;


            MpPoint normalizedlineWidenVector = new MpPoint(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            MpPoint lineWidenVector = normalizedlineWidenVector * TailWidth;

            // Adjust arrow thickness for very thick lines
            MpPoint arrowWidthVector = normalizedlineWidenVector * TipWidth;

            var pc = new List<MpPoint>();

            MpPoint endArrowCenterPosition = endPoint - (normalizedDirection * TipLength);

            // tip right base
            var p1 = endArrowCenterPosition + arrowWidthVector;
            // tail right start
            var p2 = endArrowCenterPosition + lineWidenVector;
            // tail right end
            var p3 = startPoint + lineWidenVector;
            // tail left end
            var p4 = startPoint - lineWidenVector;
            // tail left start
            var p5 = endArrowCenterPosition - lineWidenVector;
            // tip left base
            var p6 = endArrowCenterPosition - arrowWidthVector;
            // tip
            var p7 = endPoint;

            // if tail end points are inside tip don't draw tail
            var tip_tri = new MpTriangle(p7, p1, p6);
            bool show_full_arrow =
                new[] {
                    p3,
                    p4,
                }.All(x => !tip_tri.Contains(x));

            if (show_full_arrow) {
                pc.Add(p1);
                pc.Add(p2);
                pc.Add(p3);
                pc.Add(p4);
                pc.Add(p5);
                pc.Add(p6);
                pc.Add(p7);
            } else {
                pc.Add(p7);
                pc.Add(p1);
                pc.Add(p6);
            }

            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(endPoint.ToAvPoint(), true);
                pc.ForEach(x => geometryContext.LineTo(x.ToAvPoint()));
                geometryContext.EndFigure(true);
            }

            double scale = DesignerItem.Scale;
            using (ctx.PushTransform(
                    Matrix.CreateScale(scale, scale))) {
                ctx.DrawGeometry(
                    fillBrush,
                    new Pen(borderBrush, TransitionLineThickness),
                    //new Pen(Brushes.White, 3),
                    GetPointGeometry(pc.Select(x => x.ToAvPoint()), new Point()));
            }

        }

        private StreamGeometry GetPointGeometry(IEnumerable<Point> points, Point offset) {
            StreamGeometry streamGeometry = new StreamGeometry();
            if (points == null || !points.Any()) {
                return streamGeometry;
            }
            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(points.Last() + offset, true);
                points.ForEach(x => geometryContext.LineTo(x + offset));
                geometryContext.EndFigure(true);
            }

            return streamGeometry;
        }

        #endregion

        #endregion

    }
}
