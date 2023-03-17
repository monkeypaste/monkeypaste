
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
                var pavm = avm.ParentActionViewModel;
                if (pavm == null) {
                    continue;
                }

                //MpPoint tail = new MpPoint(avm.X + (avm.Width / 2), avm.Y + (avm.Height / 2));
                MpRect tail_rect = avm.ObservedDesignerItemBounds;
                var tail_adv = this.GetVisualDescendants<MpAvActionDesignerItemView>().FirstOrDefault(x => x.DataContext == avm);
                if (tail_adv != null && tail_adv.GetVisualDescendant<Shape>() is Shape tail_shape) {
                    tail_rect = tail_shape.Bounds.ToPortableRect();
                    tail_rect.Move(tail_shape.TranslatePoint(new Point(), this).Value.ToPortablePoint());
                }

                MpRect head_rect = pavm.ObservedDesignerItemBounds;
                var head_adv = this.GetVisualDescendants<MpAvActionDesignerItemView>().FirstOrDefault(x => x.DataContext == pavm);
                if (head_adv != null && head_adv.GetVisualDescendant<Shape>() is Shape head_shape) {
                    head_rect = head_shape.Bounds.ToPortableRect();
                    head_rect.Move(head_shape.TranslatePoint(new Point(), this).Value.ToPortablePoint());
                }

                //MpPoint head = new MpPoint(pavm.X + (pavm.Width / 2), pavm.Y + (pavm.Height / 2));
                MpPoint tail = tail_rect.Centroid();
                MpPoint head = head_rect.Centroid();

                var borderBrush = pavm.IsHovering ? TransitionLineHoverBorderBrush : TransitionLineDefaultBorderBrush;
                var fillBrush = GetArrowFillBrush(pavm, avm, head, tail);

                DrawArrow(dc, head, tail, avm.Width / 2, borderBrush, fillBrush);
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
                streamGeometry);
        }
    }
}
