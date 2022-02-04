using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

        public Brush TransitionLineBorderBrush { get; set; } = Brushes.White;
        public Brush TransitionLineFillBrush { get; set; } = Brushes.Red;
        public double TransitionLineThickness { get; set; } = 1;

        public double TipWidth { get; set; } = 10;

        public double TipLength { get; set; } = 20;

        public double TailWidth { get; set; } = 5;

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
                Point tail = new Point(avm.X + (avm.Width / 2), avm.Y + (avm.Height / 2));
                Point head;

                var pavm = avm.ParentActionViewModel;
                if (pavm == null) {
                    //head = avm.Parent.FindOpenDesignerLocation(tail);
                    continue;
                } else {
                    head = new Point(pavm.X + (pavm.Width / 2), pavm.Y + (pavm.Height / 2));
                }

                DrawArrow(dc, head, tail,avm.Width / 2);
            }
        }

        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }

        private void DrawArrow(DrawingContext dc, Point startPoint, Point endPoint, double dw) {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            startPoint += normalizedDirection * dw;
            endPoint -= normalizedDirection * dw;

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * TailWidth;

            // Adjust arrow thickness for very thick lines
            Vector arrowWidthVector = normalizedlineWidenVector * TipWidth;

            var pc = new PointCollection(6);

            Point endArrowCenterPosition = endPoint - (normalizedDirection * TipLength);

            //pc.Add(endPoint); // Start with tip of the arrow
            pc.Add(endArrowCenterPosition + arrowWidthVector);
            pc.Add(endArrowCenterPosition + lineWidenVector);
            pc.Add(startPoint + lineWidenVector);
            pc.Add(startPoint - lineWidenVector);
            pc.Add(endArrowCenterPosition - lineWidenVector);
            pc.Add(endArrowCenterPosition - arrowWidthVector);


            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(endPoint, true, true);
                geometryContext.PolyLineTo(pc, true, true);
            }
            streamGeometry.Freeze();
            dc.DrawGeometry(TransitionLineFillBrush, new Pen(TransitionLineBorderBrush, TransitionLineThickness), streamGeometry);

            //for (int i = 0; i < pc.Count; i++) {
            //    var p1 = pc[i];
            //    var p2 = i == pc.Count - 1 ? pc[0] : pc[i + 1];
            //    dc.DrawLine(new Pen(TransitionLineBorderBrush, TransitionLineThickness), p1,p2);
            //}
        }
    }
}
