using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpContentItemSeperatorAdorner : Adorner {
        #region Properties
        public List<List<Point>> Lines { get; set; } = new List<List<Point>>();

        public Brush Color { get; set; } = Brushes.Black;

        public double Thickness { get; set; } = 1.5;

        public DashStyle DashStyle { get; set; }

        public bool IsShowing { get; set; } = false;

        private Pen pen {
            get {
                return new Pen(Color, Thickness) { DashStyle = this.DashStyle };
            }
        }
        #endregion

        #region Public Methods
        public MpContentItemSeperatorAdorner(UIElement uie) : base(uie) {
            DashStyle = DashStyles.Dash;
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            var civm = (AdornedElement as FrameworkElement).DataContext as MpContentItemViewModel;

            if (IsShowing) {
                Visibility = Visibility.Visible;
                foreach(var l in Lines) {
                    drawingContext.DrawLine(pen, l[0], l[1]);
                }
                
            } else {
                Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
