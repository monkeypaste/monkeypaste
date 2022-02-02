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

                dc.DrawLine(new Pen(Brushes.White, 1), p1, p2);
            }
        }


        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }
    }
}
