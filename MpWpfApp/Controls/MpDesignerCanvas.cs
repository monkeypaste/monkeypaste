using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MpWpfApp {

    public class MpDesignerCanvas : Canvas {
        private DispatcherTimer _timer;
        #region Properties

        public Brush TransitionLineBrush { get; set; } = Brushes.White;
        public double TransitionLineThickness { get; set; } = 1;

        public double TipWidth { get; set; } = 0;

        public double TipLength { get; set; } = 0;
        #endregion

        public override void EndInit() {
            base.EndInit();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;

            _timer.Start();
        }


        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if(DataContext == null) {
                return;
            }

            var tavm = DataContext as MpTriggerActionViewModelBase;

            if(tavm == null) {
                return;
            }

            var avmc = tavm.FindAllChildren().ToList();

            if(avmc == null) {
                return;
            }
            avmc.Insert(0, tavm);
            foreach (var avm in avmc) {
                var pavm = avm.ParentActionViewModel;
                if (pavm == null) {
                    continue;
                }
                Point p1 = new Point(avm.X + (avm.Width / 2), avm.Y + (avm.Height / 2));
                Point p2 = new Point(pavm.X + (pavm.Width / 2), pavm.Y + (pavm.Height / 2));

                DrawArrow(dc, p2, p1,avm.Width / 2);
            }
        }

        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }

        private const double _maxArrowLengthPercent = 0.3; // factor that determines how the arrow is shortened for very short lines
        private const double _lineArrowLengthFactor = 3.73205081; // 15 degrees arrow:  = 1 / Math.Tan(15 * Math.PI / 180); 

        private void DrawArrow(DrawingContext dc, Point startPoint, Point endPoint, double dw) {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            startPoint += normalizedDirection * dw;
            endPoint -= normalizedDirection * dw;
            direction = endPoint - startPoint;

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * TransitionLineThickness * 0.5;

            double lineLength = direction.Length;

            double defaultArrowLength =  TransitionLineThickness * _lineArrowLengthFactor;

            // Prepare usedArrowLength
            // if the length is bigger than 1/3 (_maxArrowLengthPercent) of the line length adjust the arrow length to 1/3 of line length

            double usedArrowLength;
            if (lineLength * _maxArrowLengthPercent < defaultArrowLength)
                usedArrowLength = lineLength * _maxArrowLengthPercent;
            else
                usedArrowLength = defaultArrowLength;

            // Adjust arrow thickness for very thick lines
            double arrowWidthFactor;
            if (TransitionLineThickness <= 1.5)
                arrowWidthFactor = 3;
            else if (TransitionLineThickness <= 2.66)
                arrowWidthFactor = 4;
            else
                arrowWidthFactor = 1.5 * TransitionLineThickness;

            arrowWidthFactor = TipWidth == 0 ? arrowWidthFactor : TipWidth;
            Vector arrowWidthVector = normalizedlineWidenVector * arrowWidthFactor;

            usedArrowLength = TipLength == 0 ? usedArrowLength : TipLength;

            // Now we have all the vectors so we can create the arrow shape positions
            var pc = new PointCollection(7);

            Point endArrowCenterPosition = endPoint - (normalizedDirection * usedArrowLength);

            pc.Add(endPoint); // Start with tip of the arrow
            pc.Add(endArrowCenterPosition + arrowWidthVector);
            pc.Add(endArrowCenterPosition + lineWidenVector);
            pc.Add(startPoint + lineWidenVector);
            pc.Add(startPoint - lineWidenVector);
            pc.Add(endArrowCenterPosition - lineWidenVector);
            pc.Add(endArrowCenterPosition - arrowWidthVector);

            for (int i = 0; i < pc.Count; i++) {
                var p1 = pc[i];
                var p2 = i == pc.Count - 1 ? pc[0] : pc[i + 1];
                dc.DrawLine(new Pen(TransitionLineBrush, TransitionLineThickness), p1,p2);
            }
        }
    }
}
