using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvZoomBorder : MpAvUserControl<object>, MpIOverrideRender {
        // from https://stackoverflow.com/a/6782715/105028
        #region Private Variables
        private DispatcherTimer _render_timer = null;

        private MpPoint _last_mp;

        private const int _RENDER_INTERVAL_MS = 50;

        private double? _lastScale;
        private Point? _lastScaleOrigin;

        #endregion

        #region Constants
        const double WHEEL_ZOOM_DELTA = 0.1d;
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

        #region Constructors
        public MpAvZoomBorder() : base() {
            this.GestureRecognizers.Add(new PinchGestureRecognizer());
            this.AddHandler(Gestures.PinchEvent, MpAvZoomBorder_Pinch);
        }

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

        public void Zoom(double scaleDelta, Point relative_anchor) {
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

            scale = Math.Min(MaxScale, Math.Max(MinScale, scale));
            DesignerItem.Scale = scale;

            var t = new MpPoint(DesignerItem.TranslateOffsetX, DesignerItem.TranslateOffsetY) * scale;
            //relative_anchor += MpAvTriggerCollectionViewModel.Instance.DesignerCenterLocation;
            //relative_anchor *= scale;
            //MpPoint abs = (relative_anchor) + t;
            //t = abs - relative_anchor;// * scale;
            DesignerItem.TranslateOffsetX = t.X;
            DesignerItem.TranslateOffsetY = t.Y;

            if (DataContext is MpAvTriggerCollectionViewModel acvm) {
                acvm.HasModelChanged = true;
            }
        }
        public void TranslateOrigin(double deltaX, double deltaY) {
            if (DesignerItem == null) {
                return;
            }
            // NOTE to be used by DesignerItem Drop Behavior
            DesignerItem.TranslateOffsetX -= deltaX;
            DesignerItem.TranslateOffsetY -= deltaY;
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
                _render_timer.Interval = TimeSpan.FromMilliseconds(_RENDER_INTERVAL_MS);
                _render_timer.Tick += (s, e) => {
                    this.Redraw();
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

        #region Gesture Handlers

        private void MpAvZoomBorder_Pinch(object sender, PinchEventArgs e) {
            MpConsole.WriteLine($"Pinch Scale: {e.Scale} Scale Origin: {e.ScaleOrigin}");
            if (_lastScale == null && _lastScaleOrigin == null) {
                _lastScale = e.Scale;
                _lastScaleOrigin = e.ScaleOrigin;
                return;
            }

            Zoom(e.Scale - _lastScale.Value, e.ScaleOrigin - _lastScaleOrigin.Value);

            _lastScale = e.Scale;
            _lastScaleOrigin = e.ScaleOrigin;
        }
        #endregion

        #region Child Events

        private void child_MouseWheel(object sender, PointerWheelEventArgs e) {
            if (Child == null || DesignerItem == null) {
                return;
            }
            double zoom = e.Delta.Y > 0 ? WHEEL_ZOOM_DELTA : -WHEEL_ZOOM_DELTA;
            Zoom(zoom, e.GetPosition(Child));
        }

        private void child_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e) {
            if (Child != null && !MpAvMoveExtension.IsAnyMoving) {
                _last_mp = e.GetPosition(this).ToPortablePoint();
                e.Pointer.Capture(this);
                if (e.Pointer.Captured != this) {
                    var capturer = e.Pointer.Captured;
                    MpDebug.Break();
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
                    var mp = e.GetPosition(this).ToPortablePoint();
                    var v = _last_mp - mp;
                    TranslateOrigin(v.X, v.Y);
                    this.Redraw();
                    _last_mp = mp;
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

            foreach (MpAvActionViewModelBase avm in tavm.SelfAndAllDescendants.OrderBy(x => x.DesignerZIndex)) {
                //DrawActionShadow(dc, avm);

                var pavm = avm.ParentActionViewModel;
                if (pavm == null) {
                    // trigger has no arrow
                    continue;
                }

                MpRect cur_rect = GetTranslatedActionShapeRect(avm);
                MpRect parent_rect = GetTranslatedActionShapeRect(pavm);

                MpPoint tail = cur_rect.Centroid();
                MpPoint head = parent_rect.Centroid();

                var fillBrush = GetArrowFillBrush(pavm, avm, head, tail);

                double end_adjust = Math.Sqrt(Math.Pow(avm.Width / 2, 2) + Math.Pow(avm.Height / 2, 2));
                var borderBrush = pavm.IsHovering ? TransitionLineHoverBorderBrush : TransitionLineDefaultBorderBrush;
                DrawArrow(dc, head, tail, end_adjust, borderBrush, fillBrush);
            }

            //var total_rect = MpRect.Empty;
            //MpRect total_rect2 = null;
            //foreach (MpAvActionViewModelBase avm in tavm.SelfAndAllDescendants.OrderBy(x => x.DesignerZIndex)) {
            //    if (GetActionShape(avm) is Shape s &&
            //        s.GetVisualAncestor<ListBoxItem>() is ListBoxItem lbi &&
            //        s.GetVisualAncestor<ListBox>() is ListBox lb) {
            //        MpRect cur_rect = s.Bounds.ToPortableRect();
            //        var new_origin = s.TranslatePoint(new Point(), lb).Value.ToPortablePoint();
            //        cur_rect.Move(new_origin);
            //        total_rect = total_rect.Union(cur_rect);
            //        //dc.DrawRectangle(new Pen(Brushes.Cyan), cur_rect.ToAvRect());

            //        MpRect cur_rect2 = s.Bounds.ToPortableRect();
            //        var new_origin2 = s.TranslatePoint(new Point(), this).Value.ToPortablePoint();
            //        cur_rect2.Move(new_origin2);
            //        total_rect2 = total_rect2.Union(cur_rect2);
            //        //dc.DrawRectangle(new Pen(Brushes.Orange), cur_rect2.ToAvRect());
            //    }
            //}

            //var total_rect3 = MpAvTriggerCollectionViewModel.Instance.DesignerItemsRect;
            // dc.DrawRectangle(new Pen(Brushes.White), total_rect.ToAvRect());
            //dc.DrawRectangle(new Pen(Brushes.Yellow), total_rect2.ToAvRect());
            //dc.DrawRectangle(new Pen(Brushes.Red), total_rect3.ToAvRect());
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
                TransformOrigin = new RelativePoint(pp.ToAvPoint(), RelativeUnit.Absolute),
                GradientStops = new GradientStops(),
                StartPoint = new RelativePoint(pp.ToAvPoint(), RelativeUnit.Absolute),
                EndPoint = new RelativePoint(p.ToAvPoint(), RelativeUnit.Absolute)
                //StartPoint = new RelativePoint(new Point(), RelativeUnit.Relative),
                //EndPoint = new RelativePoint((p - pp).Normalized.ToAvPoint(), RelativeUnit.Relative)
                //StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                //EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative)
            };

            int strip_count = (int)((double)(pp - p).Length / 15d);

            if (pavm.IsValid) {
                fillBrush.GradientStops.Add(new GradientStop(parent_color, 0));
                fillBrush.GradientStops.Add(new GradientStop(parent_color, 0.45d));
            } else {
                fillBrush.GradientStops.AddRange(GetGradientStripes(warning_color1, warning_color2, 0, 0.5, strip_count));

                fillBrush.Transform = GetBrushTransform(fillBrush, false, pavm, p.AngleBetween(pp));
            }

            if (avm.IsValid) {
                fillBrush.GradientStops.Add(new GradientStop(cur_color, 0.55d));
                fillBrush.GradientStops.Add(new GradientStop(cur_color, 1.0d));
            } else {
                fillBrush.GradientStops.AddRange(GetGradientStripes(warning_color1, warning_color2, 0.5, 1, strip_count));
                fillBrush.Transform = GetBrushTransform(fillBrush, true, avm, p.AngleBetween(pp));
            }

            return fillBrush;
        }

        //private DateTime? _lastRenderDt = null;
        //private double _animOffset = 0;
        private ITransform GetBrushTransform(LinearGradientBrush lgb, bool is_tail, MpAvActionViewModelBase avm, double angle) {

            //_lastRenderDt = _lastRenderDt == null ? DateTime.Now : _lastRenderDt;
            //var cur_dt = DateTime.Now;
            //var dt = cur_dt - _lastRenderDt;
            //_lastRenderDt = cur_dt;
            if (avm.TagObj is not double animOffset) {
                animOffset = 0;
            } else {
                animOffset += 0.001;
            }
            double max_anim_offset = Math.Abs(lgb.GradientStops[1].Offset - lgb.GradientStops[0].Offset);
            if (animOffset > max_anim_offset) {
                animOffset = 0;
            }
            avm.TagObj = animOffset;

            //double angle = angle;//is_tail ? -90 : 0;
            Point trans = is_tail ? new Point(animOffset, 0) : new Point(0, animOffset);
            Point skew = new Point(45, -45);
            var tg = new TransformGroup();
            //tg.Children.Add(new RotateTransform(-angle));
            //tg.Children.Add(new TranslateTransform(trans.X, trans.Y));
            //tg.Children.Add(new SkewTransform(skew.X, skew.Y));
            return tg;
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
                // if tip is tail don't draw tail
                // since tail is not axis-aligned rect use 2 right triangles for test
                var tail_tri1 = new MpTriangle(p4, p5, p3);
                var tail_tri2 = new MpTriangle(p3, p5, p2);
                show_full_arrow =
                    new[] {
                        tail_tri1,
                        tail_tri2,
                    }.All(x => !x.Contains(p7));
            }

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
