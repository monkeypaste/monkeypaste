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
    public class MpRtbListBoxAdorner : Adorner {
        #region Private Variables
        #endregion

        #region Public Methods
        public MpRtbListBoxAdorner(ListBox rtblb) : base(rtblb) {
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {    
            var rtbvmc = ((FrameworkElement)this.AdornedElement).DataContext as MpClipTileRichTextBoxViewModelCollection;
            var redPen = new Pen(Brushes.Red, 1.5);
            redPen.DashStyle = DashStyles.Dash;

            var l = rtbvmc.DropLeftPoint;
            var r = rtbvmc.DropRightPoint;
            double offset = 0;
            l.X -= offset;
            r.X -= offset;

            l.Y += 0;
            r.Y -= 0;
            if(rtbvmc.HostClipTileViewModel == null) {
                return;
            }
            if (rtbvmc.HostClipTileViewModel.IsClipDropping) {
                //Console.WriteLine("Drop left point: " + l.ToString() + " drop right point: " + r.ToString());
                drawingContext.DrawLine(redPen, l, r);
            }
        }
        #endregion
    }
}
