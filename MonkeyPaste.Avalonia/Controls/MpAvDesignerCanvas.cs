
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
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
    public class MpAvDesignerCanvas : Canvas {
        #region Private Variables

        private DispatcherTimer _timer;

        #endregion

        #region Properties

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

        public override void EndInit() {
            base.EndInit();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;

            _timer.Start();
        }
        public override void Render(DrawingContext dc) {
            base.Render(dc);

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
                    continue;
                }

                MpRect tail_rect = GetTranslatedActionShapeRect(avm);
                MpRect head_rect = GetTranslatedActionShapeRect(pavm);

                MpPoint tail = tail_rect.Centroid();
                MpPoint head = head_rect.Centroid();

                var borderBrush = pavm.IsHovering ? TransitionLineHoverBorderBrush : TransitionLineDefaultBorderBrush;
                var fillBrush = GetArrowFillBrush(pavm, avm, head, tail);

                DrawArrow(dc, head, tail, avm.Width / 2, borderBrush, fillBrush);
            }
        }

        private Shape GetActionShape(MpAvActionViewModelBase avm) {
            var adv = this.GetVisualDescendants<MpAvActionDesignerItemView>().FirstOrDefault(x => x.DataContext == avm);
            if (adv != null && adv.GetVisualDescendant<Shape>() is Shape avm_shape) {
                return avm_shape;
            }
            return null;
        }
        private MpRect GetTranslatedActionShapeRect(MpAvActionViewModelBase avm) {
            if (GetActionShape(avm) is Shape s) {
                var s_rect = s.Bounds.ToPortableRect();
                s_rect.Move(s.TranslatePoint(new Point(), this).Value.ToPortablePoint());
                return s_rect;
            }
            return MpRect.Empty;
        }
        private void DrawActionShadow(DrawingContext ctx, MpAvActionViewModelBase avm) {
            MpPoint offset = new MpPoint(3, 3);
            double scale = 1;
            var origin = GetTranslatedActionShapeRect(avm).Location + offset;

            IBrush shadow_brush = new SolidColorBrush(Colors.Black, 0.1);

            Shape s = GetActionShape(avm);
            if (s is Ellipse el) {
                var r = el.Bounds.Size.ToPortableSize().ToPortablePoint() * 0.5;
                var center = origin + r;
                using (ctx.PushPostTransform(Matrix.CreateScale(scale, scale) *
                    Matrix.CreateTranslation(center.X, center.Y))) {
                    ctx.DrawEllipse(shadow_brush, new Pen(Brushes.Transparent), new Point(), r.X, r.Y);
                }
            } else if (s is Polygon pg) {
                using (ctx.PushPostTransform(
                    Matrix.CreateScale(scale, scale) *
                    Matrix.CreateTranslation(origin.X, origin.Y))) {
                    ctx.DrawGeometry(shadow_brush, new Pen(Brushes.Transparent), GetPointGeometry(pg.Points));
                }
            } else if (s is Rectangle r) {
                using (ctx.PushPostTransform(
                    Matrix.CreateScale(scale, scale) *
                    Matrix.CreateTranslation(origin.X, origin.Y))) {
                    var rect = r.Bounds.ToPortableRect();
                    rect.Move(MpPoint.Zero);
                    ctx.DrawRectangle(shadow_brush, new Pen(Brushes.Transparent), rect.ToAvRect());
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
                //fillBrush.GradientStops.Add(new GradientStop(warning_color1, 0));
                //fillBrush.GradientStops.Add(new GradientStop(warning_color2, 0.1d));

                //fillBrush.GradientStops.Add(new GradientStop(warning_color1, 0.1d));
                //fillBrush.GradientStops.Add(new GradientStop(warning_color2, 0.2d));

                //fillBrush.GradientStops.Add(new GradientStop(warning_color1, 0.2d));
                //fillBrush.GradientStops.Add(new GradientStop(warning_color2, 0.3d));

                //fillBrush.GradientStops.Add(new GradientStop(warning_color1, 0.3d));
                //fillBrush.GradientStops.Add(new GradientStop(warning_color2, 0.4d));

                //fillBrush.GradientStops.Add(new GradientStop(warning_color1, 0.4d));
                //fillBrush.GradientStops.Add(new GradientStop(warning_color1, 0.5d));
            }

            if (avm.IsValid) {
                fillBrush.GradientStops.Add(new GradientStop(cur_color, 0.55d));
                fillBrush.GradientStops.Add(new GradientStop(cur_color, 1.0d));
            } else {
                fillBrush.GradientStops.AddRange(GetGradientStripes(warning_color1, warning_color2, 0.5, 1, 7));
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

        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }

        private void DrawArrow(
            DrawingContext dc,
            MpPoint startPoint,
            MpPoint endPoint,
            double dw,
            IBrush borderBrush,
            IBrush fillBrush) {
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

            // Start with tip of the arrow
            pc.Add(endArrowCenterPosition + arrowWidthVector);
            pc.Add(endArrowCenterPosition + lineWidenVector);
            pc.Add(startPoint + lineWidenVector);
            pc.Add(startPoint - lineWidenVector);
            pc.Add(endArrowCenterPosition - lineWidenVector);
            pc.Add(endArrowCenterPosition - arrowWidthVector);
            pc.Add(endPoint);

            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(endPoint.ToAvPoint(), true);
                pc.ForEach(x => geometryContext.LineTo(x.ToAvPoint()));
                geometryContext.EndFigure(true);
            }

            dc.DrawGeometry(
                fillBrush,
                new Pen(borderBrush, TransitionLineThickness),
                GetPointGeometry(pc.Select(x => x.ToAvPoint())));
        }

        private StreamGeometry GetPointGeometry(IEnumerable<Point> points) {
            StreamGeometry streamGeometry = new StreamGeometry();
            if (points == null || !points.Any()) {
                return streamGeometry;
            }
            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(points.Last(), true);
                points.ForEach(x => geometryContext.LineTo(x));
                geometryContext.EndFigure(true);
            }

            return streamGeometry;
        }
    }
}
