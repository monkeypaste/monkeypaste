
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Wpf;
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

        public IBrush TransitionLineDefaultBorderBrush { get; set; } = Brushes.White;
        public IBrush TransitionLineHoverBorderBrush { get; set; } = Brushes.Yellow;
        public IBrush TransitionLineDisabledFillBrush { get; set; } = Brushes.Red;
        public IBrush TransitionLineEnabledFillBrush { get; set; } = Brushes.Lime;

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
            var tavm = DataContext as MpAvTriggerActionViewModelBase;
            if(tavm == null) {
                return;
            }
            foreach (MpAvActionViewModelBase avm in tavm.SelfAndAllDescendants) {
                MpPoint tail = new MpPoint(avm.X + (avm.Width / 2), avm.Y + (avm.Height / 2));

                var pavm = avm.ParentTreeItem;
                if (pavm == null) {
                    continue;
                }

                MpPoint head = new MpPoint(pavm.X + (pavm.Width / 2), pavm.Y + (pavm.Height / 2));

                var borderBrush = pavm.IsHovering ? TransitionLineHoverBorderBrush : TransitionLineDefaultBorderBrush;

                Color enabled_color = ((ImmutableSolidColorBrush)TransitionLineEnabledFillBrush).Color;
                Color disabled_color = ((ImmutableSolidColorBrush)TransitionLineDisabledFillBrush).Color;

                var parent_color = pavm.IsEnabled.IsTrue() ? enabled_color : disabled_color;
                var cur_color = avm.IsEnabled.IsTrue() ? enabled_color : disabled_color;

                var fillBrush = new LinearGradientBrush() {
                    GradientStops = new GradientStops() {
                        new GradientStop(parent_color,0),
                        new GradientStop(parent_color,0.45d),
                        new GradientStop(cur_color,0.55d),
                        new GradientStop(cur_color,1)
                    },
                    StartPoint = new RelativePoint(head.ToAvPoint(),RelativeUnit.Absolute),
                    EndPoint = new RelativePoint(tail.ToAvPoint(), RelativeUnit.Absolute)
                };

                DrawArrow(dc, head, tail, avm.Width / 2, borderBrush, fillBrush);
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
