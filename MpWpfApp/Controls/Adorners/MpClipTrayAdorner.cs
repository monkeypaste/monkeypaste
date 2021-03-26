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

        #region Public Methods
        public MpClipTrayAdorner(ListBox lb) : base(lb) {
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {          
            var clipTrayVm = ((FrameworkElement)this.AdornedElement).DataContext as MpClipTrayViewModel;
            var redPen = new Pen(Brushes.Red, 1.5);
            redPen.DashStyle = DashStyles.Dash;

            var t = clipTrayVm.DropTopPoint;
            var b = clipTrayVm.DropBottomPoint;
            double offset = 5;
           // t.X -= offset;
            //b.X -= offset;

            double trayMidY = clipTrayVm.ClipTrayListView.ActualHeight / 2;
            //t.Y += 7;
            //b.Y -= 30;
            if (clipTrayVm.IsTrayDropping) {
                drawingContext.DrawLine(redPen, t, b);
            }
        }
        #endregion
    }
}
