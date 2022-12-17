
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
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
            if(DataContext == null) {
                return;
            }

            var acvm = DataContext as MpAvActionCollectionViewModel;
            if(acvm == null) {
                return;
            }
            var tavm = acvm.SelectedItem;
            if(tavm == null) {
                return;
            }

            var avmc = tavm.FindAllChildren().ToList();
            if(avmc == null) {
                return;
            }
            avmc.Insert(0, tavm);
            foreach (MpAvActionViewModelBase avm in avmc) {
                Point tail = new Point(avm.X + (avm.Width / 2), avm.Y + (avm.Height / 2));


                var pavm = avm.ParentActionViewModel;
                if (pavm == null) {
                    continue;
                }

                var borderBrush = pavm.IsHovering ? TransitionLineHoverBorderBrush : TransitionLineDefaultBorderBrush;
                var fillBrush = avm.IsEnabled.HasValue && avm.IsEnabled.Value ? //&&
                                //(pavm.ParentActionViewModel == null || (pavm.ParentActionViewModel.IsEnabled.HasValue && pavm.ParentActionViewModel.IsEnabled.Value)) ?
                    TransitionLineEnabledFillBrush : TransitionLineDisabledFillBrush;

                Point head = new Point(pavm.X + (pavm.Width / 2), pavm.Y + (pavm.Height / 2));

                DrawArrow(dc, head, tail, avm.Width / 2, borderBrush, fillBrush);
            }
        }

        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }

        private void DrawArrow(DrawingContext dc, Point startPoint, Point endPoint, double dw, IBrush borderBrush, IBrush fillBrush) {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            startPoint += normalizedDirection * dw;
            endPoint -= normalizedDirection * dw;

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * TailWidth;

            // Adjust arrow thickness for very thick lines
            Vector arrowWidthVector = normalizedlineWidenVector * TipWidth;

            var pc = new List<Point>();

            Point endArrowCenterPosition = endPoint - (normalizedDirection * TipLength);

            // Start with tip of the arrow
            pc.Add(endArrowCenterPosition + arrowWidthVector);
            pc.Add(endArrowCenterPosition + lineWidenVector);
            pc.Add(startPoint + lineWidenVector);
            pc.Add(startPoint - lineWidenVector);
            pc.Add(endArrowCenterPosition - lineWidenVector);
            pc.Add(endArrowCenterPosition - arrowWidthVector);


            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(endPoint, true);
                pc.ForEach(x => geometryContext.LineTo(x));
                geometryContext.EndFigure(true);
            }

            dc.DrawGeometry(
                fillBrush,
                new Pen(borderBrush, TransitionLineThickness),
                streamGeometry);
        }
    }
}
