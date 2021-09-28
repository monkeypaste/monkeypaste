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
    public class MpLineAdorner : Adorner {

        #region Properties
        public Point[] Points { get; set; } = new Point[2];

        public Brush Color { get; set; } = Brushes.Red;

        public double Thickness { get; set; } = 5;

        public DashStyle DashStyle { get; set; }

        public bool IsShowing { get; set; } = false;

        private Pen pen {
            get {
                return new Pen(Color, Thickness) { DashStyle = this.DashStyle };
            }
        }
        #endregion

        #region Public Methods
        public MpLineAdorner(UIElement uie) : base(uie) {
            DashStyle = DashStyles.Dash; 
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            if(IsShowing) {
                Visibility = Visibility.Visible; 
                drawingContext.DrawLine(pen, Points[0], Points[1]);
            } else {
                Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
