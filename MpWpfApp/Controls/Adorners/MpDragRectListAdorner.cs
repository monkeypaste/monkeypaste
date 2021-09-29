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
    public class MpDragRectListAdorner : Adorner {
        #region Properties
        public List<Rect> RectList { get; set; } = new List<Rect>();

        public Brush Color { get; set; } = Brushes.Pink;

        public double Thickness { get; set; } = 0.5;

        public DashStyle DashStyle { get; set; }

        public bool IsShowing { get; set; } = false;

        private Pen pen {
            get {
                return new Pen(Color, Thickness) { DashStyle = this.DashStyle };
            }
        }

        private Brush brush {
            get {
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Public Methods
        public MpDragRectListAdorner(UIElement uie) : base(uie) {
            DashStyle = DashStyles.Dash; 
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            if(RectList.Count > 0) {
                Visibility = Visibility.Visible; 
                foreach(var rect in RectList) {
                    drawingContext.DrawRectangle(brush, pen, rect);
                }
            } else {
                Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
