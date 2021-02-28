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
    public class MpRichTextBoxOverlayAdorner : Adorner {
        #region Private Variables
        private Canvas _rtbc;
        private RichTextBox _rtb;
        private ListBox _rtblb;
        #endregion

        #region Public Methods
        public MpRichTextBoxOverlayAdorner(Canvas rtbc) : base(rtbc) {
            _rtbc = rtbc;
            _rtb = (RichTextBox)_rtbc.FindName("ClipTileRichTextBox");
            _rtblb = rtbc.GetVisualAncestor<ListBox>();
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            var rtbvm = (MpClipTileViewModel)_rtbc.DataContext;  
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var blackPen = new Pen(Brushes.Gray, 1);
            blackPen.DashStyle = DashStyles.Dash;
            var redPen = new Pen(Brushes.Red, 2);
            
            if (rtbvm.Previous != null) {
                drawingContext.DrawLine(blackPen, adornedElementRect.TopLeft, adornedElementRect.TopRight);
            }
            if (rtbvm.Next != null) {
                drawingContext.DrawLine(blackPen, adornedElementRect.BottomLeft, adornedElementRect.BottomRight);
            }
        }
        #endregion
    }
}
