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
    public class MpRtbAdorner : Adorner {
        #region Private Variables
        #endregion

        #region Statics
        
        #endregion

        #region Public Methods
        public MpRtbAdorner(RichTextBox rtb) : base(rtb) {
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {            
            var rtbvm = (MpRtbListBoxItemRichTextBoxViewModel)(this.AdornedElement as FrameworkElement).DataContext;
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var redPen = new Pen(Brushes.Red, 1);

            if (rtbvm.IsSubTextDropping) {                
                drawingContext.DrawLine(redPen, rtbvm.DropTopPoint, rtbvm.DropBottomPoint);
            }
        }
        #endregion
    }
}
