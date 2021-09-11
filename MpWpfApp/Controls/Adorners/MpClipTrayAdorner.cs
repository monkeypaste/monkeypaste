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
    public class MpClipTrayAdorner : Adorner {
        #region Private Variables
        #endregion

        #region Statics

        #endregion

        #region Properties
        public Point DropTopPoint { get; set; }
        public Point DropBottomPoint { get; set; }
        #endregion

        #region Public Methods
        public MpClipTrayAdorner(ListBox lb) : base(lb) { }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {          
            var clipTrayVm = ((FrameworkElement)this.AdornedElement).DataContext as MpClipTrayViewModel;
            var redPen = new Pen(Brushes.Red, 1.5);
            redPen.DashStyle = DashStyles.Dash;

            if (clipTrayVm.IsTrayDropping) {
                drawingContext.DrawLine(redPen, DropTopPoint, DropBottomPoint);
            }
        }
        #endregion
    }
}
