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
        public Point Point1 { get; set; }
        
        public Point Point2 { get; set; }

        public Brush Color { get; set; } = Brushes.Red;

        public double Thickness { get; set; } = 1.5;

        public DashStyle DashStyle { get; set; }

        public bool IsShowing { get; set; } = false;

        private Pen _pen {
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
                drawingContext.DrawLine(_pen, Point1, Point2);
            }
        }
        #endregion
    }
}
